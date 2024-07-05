using CommonAPI;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BepInEx.Logging;

namespace EmailApi
{
    public class EmailSave : CustomSaveData
    {
        private static ManualLogSource DebugLog = BepInEx.Logging.Logger.CreateLogSource($"{PluginInfo.PLUGIN_NAME} Email Manager Save");

        public static EmailSave Instance { get; private set; }

        private readonly Dictionary<string, bool> messageStates;
        
        public EmailSave() : base(PluginInfo.PLUGIN_NAME, "{0}_msg.data", SaveLocations.Documents)
        {
            Instance = this;
            messageStates = new Dictionary<string, bool>();
            DebugLog.LogMessage("Start Email Save");
            DebugLog.LogMessage(this.Filename);
        }

        public bool hasReadData = false;

        public void setMessageState(string messageName, bool state)
        {
            if (messageStates.ContainsKey(messageName))
            {
                messageStates[messageName] = state;
                EmailSave.Instance.Save();
                DebugLog.LogMessage("Update then Save Email");
                return;
            }
            messageStates.Add(messageName, state);
            EmailSave.Instance.Save();
            DebugLog.LogMessage("Add new then Save Email");
        }

        public bool getMessageState(string messageName)
        {
            DebugLog.LogMessage($"Get messageName| {messageName}");
            if (messageStates.ContainsKey(messageName))
            {
                return messageStates[messageName];
            }
            DebugLog.LogMessage($"Failed to find Message");
            return false;
        }

        // Starting a new save - start from zero.
        public override void Initialize()
        {
            DebugLog.LogMessage("EmailManager Init");
            messageStates.Clear();
        }
        public override void Read(BinaryReader reader)
        {
            var version = reader.ReadByte();          
            var messages = reader.ReadInt32();

            for (var i = 0; i < messages; i++)
            {
                var msgID = reader.ReadString();
                var msgState = reader.ReadBoolean();

                if (!messageStates.ContainsKey(msgID))
                {
                    messageStates.Add(msgID, msgState);
                }
                
            }
            hasReadData = true;
        }
        public override void Write(BinaryWriter writer)
        {
            // Version
            writer.Write((byte)0);
            writer.Write(messageStates.Count);
            foreach (var messageState in messageStates)
            {
                writer.Write(messageState.Key);
                writer.Write(messageState.Value); 
            }
        }
    }
}

