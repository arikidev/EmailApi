using UnityEngine;
using System.Collections.Generic;
using Reptile;
using System;
using BepInEx.Logging;
using BepInEx;
using CommonAPI;
using Reptile.Phone;
using HarmonyLib;

namespace EmailApi
{
    public static class EmailManager
    {
        private static ManualLogSource DebugLog = BepInEx.Logging.Logger.CreateLogSource($"{PluginInfo.PLUGIN_NAME} EmailManager");

        //Remove should be set by user
        static int msgSenderID = 9001;
        static UnityEngine.Color msgSenderColor = new UnityEngine.Color(1f, 0.9f, 0.9f);
        public static EmailMessage msg_tutorial;

        public static UnityEngine.Color[] msgSenderColors = new UnityEngine.Color[] { new Color(1f, 1f, 1f, 1f), new Color(.4863f, 0.5569f, 0.78f, 1f) };
        public static UnityEngine.Color[] msgSubjectColors = new UnityEngine.Color[] { new Color(0.196f, 0.31f, 0.612f, 1f), new Color(1f, 1f, 1f, 1f) };


        public static Player player;
        public static Phone phone;
        public static AppEmail emailApp;
  
        public static Dictionary<int, CustomContact> _Contacts { get; private set; }

        public static List<KeyValuePair<EmailMessage, bool>> _messagesDatabase;
        public static Dictionary<string, EmailMessage> _messagesIDDatabase;

        private static EmailSave SaveState;
        private static EmailApiPlugin plugin;

        public static void Initialize(EmailApiPlugin _plugin)
        {
            DebugLog.LogMessage($"Init Email Manager");

            //player = WorldHandler.instance.GetCurrentPlayer();
            //phone = (Phone)player.GetField("phone").GetValue(player);
            //emailApp = phone.GetAppInstance<AppEmail>();

            plugin = _plugin;

            InitLoadEmailSave();
            InitCustomContacts();
            InitEmailMessages();
        }

        private static void InitLoadEmailSave()
        {
            new EmailSave();

            if (EmailSave.Instance.hasReadData)
            { 
                
            }
        }

        private static void InitCustomContacts()
        {
            _Contacts = new Dictionary<int, CustomContact>();
        }

        public static CustomContact RegisterCustomContact(string contactName, int contactID, Texture2D contactImage )
        {
            CustomContact _Contact = new CustomContact(contactID, contactName);
            _Contact.avatar = TextureUtility.CreateSpriteFromTexture(contactImage);
            _Contacts.Add(_Contact.characterID, _Contact);

            DebugLog.LogMessage($"Added Contact {contactName}");

            return _Contact;
        }

        private static void InitEmailMessages()
        {
            _messagesDatabase = new List<KeyValuePair<EmailMessage, bool>>();
            _messagesIDDatabase = new Dictionary<string, EmailMessage>();
        }

        public static void AddEmailMessage(EmailMessage email, bool checkSave = false)
        {
            //CheckSave
            // TODO
            
            _messagesDatabase.Add(new KeyValuePair<EmailMessage, bool>(email, checkSave));
            _messagesIDDatabase.Add(email.name, email);
        }

        public static EmailMessage CreateEmailMessage(string msgID, int msgSenderID, string subject, UnityEngine.Color msgSenderColor , List<string> messages)
        {
            EmailMessage msgObj = ScriptableObject.CreateInstance<EmailMessage>();
            msgObj.name = msgID;
            msgObj.characterNameIDOfSender = msgSenderID;
            msgObj.subject = subject;
            msgObj.profileBackground = msgSenderColor;
            msgObj.message = messages.ToArray();

            return msgObj;
        }

        public static void EmailNotification(string MessageID, bool setSave = true )
        {
            player = WorldHandler.instance.GetCurrentPlayer();
            phone = player.phone;
            emailApp = phone.GetAppInstance<AppEmail>();

            if (emailApp != null)
            {
                if (_messagesIDDatabase.TryGetValue(MessageID, out EmailMessage message))
                {
                    if (setSave)
                    {
                        EmailSave.Instance.setMessageState(MessageID, setSave);
                    }

                    emailApp.PushNotification(message);
                }
                else 
                {
                    DebugLog.LogMessage($"Failed to find email of ID: {MessageID}" );
                }
            }    
        }

        public static void EmailNotificationDelayed(string MessageID, bool setSave = true, float delay = 2.0f)
        {
            if (true)//(plugin != null)
            {
                DebugLog.LogMessage($"setup delayed Email Notification");
                plugin.DelayEmailNotification(MessageID, setSave, delay);
            }
        }
    }

    public struct CustomContact
    {
        public int CompareTo(Contact other)
        {
            return this.characterID.CompareTo(other.characterID);
        }

        public CustomContact(int characterID, string characterName)
        {
            this.characterID = characterID;
            this.characterName = characterName;
            this.avatar = null;
            this.associatedCharacterGuid = Guid.Empty;

        }

        // Token: 0x04001D98 RID: 7576
        public int characterID;

        public string characterName;

        // Token: 0x04001D99 RID: 7577
        public Sprite avatar;

        public Guid associatedCharacterGuid;

    }

}
