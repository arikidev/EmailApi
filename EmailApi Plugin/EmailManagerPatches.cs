using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Reptile;
using Reptile.Phone;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System;
using UnityEngine.UI;

namespace EmailApi
{

    [HarmonyPatch(typeof(Reptile.Phone.AppEmail))]
    public class PhoneAppEmailPatch
    {
        private static ManualLogSource DebugLog = BepInEx.Logging.Logger.CreateLogSource($"{PluginInfo.PLUGIN_NAME} AppEmail Patch");

        [HarmonyPostfix]
        [HarmonyPatch("RefreshList")]
        public static void RefreshListpatch(AppEmail __instance, ref List<EmailMessage> ___m_Emails, ref EmailScrollView ___m_CardsScroll)
        {
            ___m_Emails.Reverse();

            foreach (KeyValuePair<EmailMessage, bool> messageInfo in EmailManager._messagesDatabase)
            {
                //Check if message is active using Save
                if (EmailSave.Instance.getMessageState(messageInfo.Key.name))
                {
                    //Check if message has already been added
                    if (!___m_Emails.Contains(messageInfo.Key))
                    {
                        ___m_Emails.Add(messageInfo.Key);
                    }
                }
            }
            ___m_Emails.Reverse();

            ___m_CardsScroll.UpdateListContent(___m_Emails.Count);
        }

        [HarmonyPrefix]
        [HarmonyPatch("PushNotification")]
        public static bool PushNotificationPatch(AppEmail __instance, EmailMessage email)
        {
            if (email.characterNameIDOfSender > 9000)
            {

                if(EmailManager._Contacts.TryGetValue(email.characterNameIDOfSender, out CustomContact contact))
                {
                    EmailManager.phone.PushNotification(__instance, contact.characterName, email);
                }

                return false;
            }
                return true;
        }
    }

    [HarmonyPatch(typeof(Reptile.Phone.EmailCard))]
    public class EmailCardPatch
    {
        private static ManualLogSource DebugLog = BepInEx.Logging.Logger.CreateLogSource($"{PluginInfo.PLUGIN_NAME} EmailCard");

        [HarmonyPrefix]
        [HarmonyPatch("SetContent")]
        public static bool SetContentpatch(EmailCard __instance, AUnlockable content, ref Image ___m_ProfilePicture, ref Image ___m_BackgroundImage)
        {
            EmailMessage emailMessage = (content as EmailMessage);
            
            if (emailMessage != null)
            {
                // Custom Email Sender ID should be greater that 9000
                if (emailMessage.characterNameIDOfSender > 9000)
                {
                    var property = typeof(EmailCard).GetProperty("AssignedContent", BindingFlags.Public | BindingFlags.Instance);
                    property = property.DeclaringType.GetProperty(property.Name);
                    property.SetValue(__instance, content);

                    TextMeshProUGUI Subject = __instance.m_SubjectLabel;
                    TextMeshProUGUI Sender = __instance.m_SenderLabel;

                    Subject.text = emailMessage.subject;
                    string name = emailMessage.characterNameIDOfSender.ToString();

                    if (EmailManager._Contacts.TryGetValue(emailMessage.characterNameIDOfSender, out CustomContact contact ))
                    {
                        name = contact.characterName;
                        ___m_ProfilePicture.sprite = contact.avatar;

                        __instance.m_SenderColors = EmailManager.msgSenderColors;
                        __instance.m_SubjectColors = EmailManager.msgSubjectColors;
                    }
                    Sender.text = name;

                    return false;
                }

            }

            return true;
        }
    }

    [HarmonyPatch(typeof(EmailMessagePanel))]
    class EmailMessagePanelPatch
    {
        private static ManualLogSource DebugLog = BepInEx.Logging.Logger.CreateLogSource($"{PluginInfo.PLUGIN_NAME} EmailCard");

        [HarmonyPrefix]
        [HarmonyPatch("SetContent")]
        public static bool SetContentpatch(EmailMessagePanel __instance, EmailMessage email, 
            ref int ___messageCurrentPage, 
            ref int ___messagePageCount, 
            ref EmailMessage ___currentEmail, 
            ref TextMeshProUGUI ___senderText,
            ref TextMeshProUGUI ___subjectText, 
            ref TextMeshProUGUI ___messageArea,
            ref Image ___senderAvatar)
        {
            if (email.characterNameIDOfSender > 9000)
            {
                ___messageCurrentPage = 0;
                ___messagePageCount = email.message.Length;
                ___currentEmail = email;
                ___senderText.text = email.characterNameIDOfSender.ToString();
                ___subjectText.text = email.subject; 
                
                if (EmailManager._Contacts.TryGetValue(email.characterNameIDOfSender, out CustomContact contact))
                {
                    ___senderText.text = contact.characterName;
                    ___senderAvatar.sprite = contact.avatar;
                }

                ___messageArea.gameObject.SetActive(false);

                return false;
            }

            return true;
        }
    }

    /**  Trigger on startUp.
    [HarmonyPatch(typeof(Player))]
    class PlayerEmailPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Init")]
        public static void InitPatch()
        {
            //If it has not been read then try the email notification for movestyler
            if (!EmailSave.Instance.getMessageState("msg1"))
            {
                EmailManager.EmailNotification("msg1", true);
            }

        }
    }
    **/
}
