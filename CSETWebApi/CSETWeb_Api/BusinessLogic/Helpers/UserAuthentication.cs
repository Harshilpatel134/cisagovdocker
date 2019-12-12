//////////////////////////////// 
// 
//   Copyright 2019 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWeb_Api.BusinessLogic.AssessmentIO;
using CSETWeb_Api.BusinessManagers;
using CSETWeb_Api.Models;
using DataLayerCore.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace CSETWeb_Api.Helpers
{
    /// <summary>
    /// Authenticates an instance of Login against the USER database table.
    /// </summary>
    public class UserAuthentication
    {
        public static LoginResponse Authenticate(Login login)
        {
            // Ensure that we have what we need
            if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return null;
            }

            USERS loginUser = null;

            // Read directly from the database; UserManager does not read password and salt, in order to keep them more private
            using (var db = new CSET_Context())
            {
                loginUser = db.USERS.Where(x => x.PrimaryEmail == login.Email).FirstOrDefault();

                if (loginUser == null)
                {
                    return null;
                }
            }

            // Validate the supplied password against the hashed password and its salt
            bool success = PasswordHash.ValidatePassword(login.Password, loginUser.Password, loginUser.Salt);
            if (!success)
            {
                return null;
            }

            // Generate a token for this user
            string token = TransactionSecurity.GenerateToken(loginUser.UserId, login.TzOffset, -1, null, login.Scope);

            // Build response object
            LoginResponse resp = new LoginResponse
            {
                Token = token,
                UserId = loginUser.UserId,
                Email = login.Email,
                UserFirstName = loginUser.FirstName,
                UserLastName = loginUser.LastName,
                IsSuperUser = loginUser.IsSuperUser,
                ResetRequired = loginUser.PasswordResetRequired ?? true,
                ExportExtension = IOHelper.GetFileExtension(login.Scope)
            };

            return resp;
        }


        /// <summary>
        /// Emulates credential authentication without requiring credentials.
        /// The Windows file system is consulted to see if a certain file was placed there
        /// during the stand-alone install process.  
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public static LoginResponse AuthenticateStandalone(Login login)
        {
            int userIdSO = 100;
            string primaryEmailSO = "";

            // Read the file system for the LOCAL-INSTALLATION file put there at install time
            if (!IsLocalInstallation(login.Scope))
            {
                return null;
            }


            String name = WindowsIdentity.GetCurrent().Name;
            name = string.IsNullOrWhiteSpace(name) ? "Local" : name;
            primaryEmailSO = name + "@myorg.org";
            using (var db = new CSET_Context())
            {
                var user = db.USERS.Where(x => x.PrimaryEmail == primaryEmailSO).FirstOrDefault();
                if (user == null)
                {
                    UserManager um = new UserManager();
                    UserDetail ud = new UserDetail()
                    {
                        Email = primaryEmailSO,
                        FirstName = name,
                        LastName = ""
                    };
                    UserCreateResponse userCreateResponse = um.CreateUser(ud);

                    db.SaveChanges();
                    //update the userid 1 to the new user
                    var tempu = db.USERS.Where(x => x.PrimaryEmail == primaryEmailSO).FirstOrDefault();
                    if (tempu != null)
                        userIdSO = tempu.UserId;
                    determineIfUpgradedNeededAndDoSo(userIdSO);
                }
                else
                {
                    userIdSO = user.UserId;
                }
            }

            if (string.IsNullOrEmpty(primaryEmailSO))
            {
                return null;
            }


            // Generate a token for this user
            string token = TransactionSecurity.GenerateToken(userIdSO, login.TzOffset, -1, null, login.Scope);

            // Build response object
            LoginResponse resp = new LoginResponse
            {
                Token = token,
                Email = primaryEmailSO,
                UserFirstName = name,
                UserLastName = "",
                IsSuperUser = false,
                ResetRequired = false,
                ExportExtension = IOHelper.GetFileExtension(login.Scope)
            };


            return resp;
        }

        private static bool IsUpgraded = false;

        private static void determineIfUpgradedNeededAndDoSo(int newuserID)
        {
            //look to see if the localuser@myorg.org exists
            //if so then get that user id and changes all 
            if (!IsUpgraded)
            {
                using (var db = new CSET_Context())
                {
                    var user = db.USERS.Where(x => x.PrimaryEmail == "localuser@myorg.org").FirstOrDefault();
                    if (user != null)
                    {
                        db.Database.ExecuteSqlCommand("update assessment_contacts set userid = @newId where userid = @oldId",
                            new SqlParameter("@newId", newuserID),
                            new SqlParameter("@oldId", user.UserId)
                        );
                    }
                }
            }
            IsUpgraded = true;
        }


        /// <summary>
        /// Returns 'true' if the installation is 'local' (self-contained using Windows identity).
        /// The local installer will place an empty file (LOCAL-INSTALLATION) in the website folder
        /// and the existence of the file indicates if the installation is local.
        /// </summary>
        /// <param name="app_code"></param>
        /// <returns></returns>
        public static bool IsLocalInstallation(String app_code)
        {
            string physicalAppPath = HttpContext.Current.Request.PhysicalApplicationPath;

            return File.Exists(Path.Combine(physicalAppPath,"LOCAL-INSTALLATION"));
        }
    }
}

