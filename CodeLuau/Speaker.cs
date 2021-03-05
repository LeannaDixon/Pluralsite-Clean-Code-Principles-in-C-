﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeLuau
{
	/// <summary>
	/// Represents a single speaker
	/// </summary>
	public class Speaker
	{
        /* TODO: Extract RegisterValidator.cs
        Eliminate hard coded arrays using a db.*/

        public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public int? yearsOfExperience { get; set; }
		public bool HasBlog { get; set; }
		public string BlogURL { get; set; }
		public WebBrowser Browser { get; set; }
		public List<string> Certifications { get; set; }
		public string Employer { get; set; }
		public int RegistrationFee { get; set; }
		public List<Session> Sessions { get; set; }

		/// <summary>
		/// Register a speaker
		/// </summary>
		/// <returns>speakerID</returns>
		public RegisterResponse Register(IRepository repository)
        {
            var error = ValidateRegistration();
            if (error != null)
                return new RegisterResponse(error);

            int? speakerId = repository.SaveSpeaker(this);
                return new RegisterResponse((int)speakerId);
        }

        private RegisterError? ValidateRegistration()
        {
            var error = ValidateData();
            if (error != null) 
                return error;

            bool speakerAppearsQualified = AppearsExceptional() || !HasObviousRedFlags();
            if (!speakerAppearsQualified)
                return RegisterError.SpeakerDoesNotMeetStandards;

            if (!ApproveSessions()) 
                return RegisterError.NoSessionsApproved;

            return null;
        }

        private RegisterError? ValidateData()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return RegisterError.FirstNameRequired;

            if (string.IsNullOrWhiteSpace(LastName))
                return RegisterError.LastNameRequired;

            if (string.IsNullOrWhiteSpace(Email))
                return RegisterError.EmailRequired;

            if (!Sessions.Any())
                return RegisterError.NoSessionsProvided;

            return null;
        }

        private bool AppearsExceptional()
        {
            if (yearsOfExperience > 10)
                return true;
            if (HasBlog)
                return true;
            if (Certifications.Count() > 3)
                return true;

            var preferredEmployers = new List<string>() { "Pluralsight", "Microsoft", "Google" };
            if (preferredEmployers.Contains(Employer))
                return true;

            return false;
        }

        private bool HasObviousRedFlags()
        {
            string emailDomain = Email.Split('@').Last();
            var ancientEmailDomains = new List<string>() { "aol.com", "prodigy.com", "compuserve.com" };

            if (ancientEmailDomains.Contains(emailDomain))
                return true;
            if (Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9)
                return true;

            return false;
        }

        private bool SessionIsAboutOldTechnologies(Session session)
        {
            var oldTechnologies = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };
            foreach (var tech in oldTechnologies)
            {
                if (session.Title.Contains(tech) || session.Description.Contains(tech))
                    return true;
            }
            return false;
        }

        private bool ApproveSessions()
        {
            foreach (var session in Sessions)
            {
                session.Approved = !SessionIsAboutOldTechnologies(session);
            }
            return Sessions.Any(s => s.Approved);
        }
    }
}