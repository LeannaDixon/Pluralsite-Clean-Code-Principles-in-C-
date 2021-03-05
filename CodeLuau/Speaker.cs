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
            int? speakerId = null;

            var error = ValidateData();
            if (error != null) return new RegisterResponse(error);

            bool speakerAppearsQualified = AppearsExceptional() || !HasObviousRedFlags();

            if (!speakerAppearsQualified)
            {
                return new RegisterResponse(RegisterError.SpeakerDoesNotMeetStandards);
            }
            bool approved = false;

            foreach (var session in Sessions)
            {
                var ot = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };

                foreach (var tech in ot)
                {
                    if (session.Title.Contains(tech) || session.Description.Contains(tech))
                    {
                        session.Approved = false;
                        break;
                    }
                    else
                    {
                        session.Approved = true;
                        approved = true;
                    }
                }
            }

            if (approved)
            {
                //if we got this far, the speaker is approved
                //let's go ahead and register him/her now.
                //First, let's calculate the registration fee. 
                //More experienced speakers pay a lower fee.
                if (yearsOfExperience <= 1)
                {
                    RegistrationFee = 500;
                }
                else if (yearsOfExperience >= 2 && yearsOfExperience <= 3)
                {
                    RegistrationFee = 250;
                }
                else if (yearsOfExperience >= 4 && yearsOfExperience <= 5)
                {
                    RegistrationFee = 100;
                }
                else if (yearsOfExperience >= 6 && yearsOfExperience <= 9)
                {
                    RegistrationFee = 50;
                }
                else
                {
                    RegistrationFee = 0;
                }


                //Now, save the speaker and sessions to the db.
                try
                {
                    speakerId = repository.SaveSpeaker(this);
                }
                catch (Exception e)
                {
                    //in case the db call fails 
                }
            }
            else
            {
                return new RegisterResponse(RegisterError.NoSessionsApproved);
            }
            //if we got this far, the speaker is registered.
            return new RegisterResponse((int)speakerId);
        }

        private bool HasObviousRedFlags()
        {
            string emailDomain = Email.Split('@').Last();
            var ancientEmailDomains = new List<string>() { "aol.com", "prodigy.com", "compuserve.com" };

            if (ancientEmailDomains.Contains(emailDomain)) return true;
            if (Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9) return true;

            return false;
        }

        private bool AppearsExceptional()
        {
            if (yearsOfExperience > 10) return true;
            if (HasBlog) return true;
            if (Certifications.Count() > 3) return true;

            var preferredEmployers = new List<string>() { "Pluralsight", "Microsoft", "Google" };
            if (preferredEmployers.Contains(Employer)) return true;
            return false;
        }

        private RegisterError? ValidateData()
        {
            // BEts to return an array.
            if (string.IsNullOrWhiteSpace(FirstName)) return RegisterError.FirstNameRequired;
            if (string.IsNullOrWhiteSpace(LastName))  return RegisterError.LastNameRequired;
            if (string.IsNullOrWhiteSpace(Email)) return RegisterError.EmailRequired;
            if (!Sessions.Any())
            {
                return RegisterError.NoSessionsProvided;
            }
            return null;
        }
	}
}