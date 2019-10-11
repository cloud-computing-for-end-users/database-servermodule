using System;
using System.Linq;
using custom_message_based_implementation;
using custom_message_based_implementation.interfaces;
using custom_message_based_implementation.model;
using database_servermodule.Models;
using message_based_communication.model;
using message_based_communication.module;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace database_servermodule
{
    public class DatabaseServermodule : BaseServerModule, IDatabaseServermodule
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly CCFEU_DBContext _context;
        public DatabaseServermodule(ModuleType moduleType) : base(moduleType)
        {
            _context = new CCFEU_DBContext();
        }

        public override string CALL_ID_PREFIX => "DATABASE_SM_CALL_ID_";

        
        public override void HandleRequest(BaseRequest message)
        {
            object responsePayload;

            switch (message)
            {
                case RequestLogin reqLogin:
                    responsePayload = Login(reqLogin.Email, reqLogin.Password);
                    break;
                case RequestCreateAccount reqCreateAccount:
                    responsePayload = CreateAccount(reqCreateAccount.Email, reqCreateAccount.Password);
                    break;
                default:
                    throw new Exception("Received message that I don't know how to handle");
            }

            var response = GenerateResponseBasedOnRequestAndPayload(message, responsePayload);
            SendResponse(response);
        }

        public PrimaryKey Login(Email email, Password password)
        {
            Logger.Debug("Select user: " + email.TheEmail + "; " + password.ThePassword);
            var list = _context.User.Where(u => u.Email.Equals(email.TheEmail) && u.Password.Equals(password.ThePassword)).ToList();

            if (list.Count == 1)
            {
                Logger.Debug("Returning PK: " + list[0].UserId);
                return new PrimaryKey { TheKey = list[0].UserId };
            }
            Logger.Debug("User not found, returning PK: -2 ");
            return new PrimaryKey { TheKey = -2 };
        }

        public PrimaryKey CreateAccount(Email email, Password password)
        {
            Logger.Debug("Adding new user into DB: " + email.TheEmail + "; " + password.ThePassword);
            var user = new User()
            {
                Email = email.TheEmail,
                Password = password.ThePassword
            };
            try
            {
                var e = _context.User.Add(user);
                _context.SaveChanges();
                Logger.Debug("Added new user into DB, PK: " + e.Entity.UserId);

                return new PrimaryKey{TheKey = e.Entity.UserId };
            }
            catch (DbUpdateException e)
            {
                Logger.Debug(e);
                Console.WriteLine(e.ToString());

                _context.User.Remove(user);
                Logger.Debug("Email not unique (most likely), returning PK: -2");
                return new PrimaryKey { TheKey = -2 };
            }
        }
    }
}