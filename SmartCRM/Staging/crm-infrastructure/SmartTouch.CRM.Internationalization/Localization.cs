﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartTouch.CRM.Internationalization
{
    /// <summary>
    /// Holds all the information necessary to Localize the site into a single language.
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// A list of translated messages
        /// </summary>
        public Dictionary<string, Message> Messages { get; set; }

        /// <summary>
        /// Translated messages, indexed by a short, autogenerated ID.
        /// Used during .po editing to avoid needing to pass around giant string keys.
        /// </summary>
        public Dictionary<int, Message> MessagesByAutoID { get; set; }

        /// <summary>
        /// Whether comment information should be loaded and stored in memory for this Localization.
        /// Generally only used if you plan to use this Localization object to edit and save to a .po file.
        /// </summary>
        public bool LoadComments { get; set; }

        /// <summary>
        /// Creates a new, empty, Localization
        /// </summary>
        public Localization()
        {
            Messages = new Dictionary<string, Message>();
            MessagesByAutoID = new Dictionary<int, Message>();
            LoadComments = false;
        }

        /// <summary>
        /// Creates a new Localization, populated from a .po file at the supplied filePath
        /// </summary>
        /// <param name="filePath"></param>
        public Localization(string filePath)
            : this()
        {
            LoadFromFile(filePath);
        }

        /// <summary>
        /// Creates a new Localization, populated from a .po file at the supplied filePath.
        /// If requested, will also load comments from that file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="loadComments">If true, will populate comments so that this .po file can be saved again exactly as it was loaded.</param>
        public Localization(string filePath, bool loadComments)
            : this()
        {
            LoadComments = loadComments;
            LoadFromFile(filePath);
        }

        /// <summary>
        /// Add Localization messages from a .po file.
        /// Doesn't initialize the collection, so it can be called from
        /// a loop if needed.
        /// </summary>
        /// <param name="filepath"></param>
        public void LoadFromFile(string filepath)
        {
            LoadFromReader(new StreamReader(filepath, Encoding.UTF8, true));
        }

        /// <summary>
        /// Add Localization messages from the specified TextReader 
        /// (which hopefully points at a .po file)
        /// Doesn't initialize the collection, so it can be called from
        /// a loop if needed.
        /// </summary>
        /// <param name="reader"></param>
        public void LoadFromReader(TextReader reader)
        {
            Regex re = new Regex(@"""(.*)""", RegexOptions.Compiled);

            string line;
            bool parsingMsgID = true;
            Message message = new Message();
            line = reader.ReadLine();
            while (null != (line))
            {
                if (String.IsNullOrEmpty(line))
                {
                    // new message block
                    if (message.MsgID != "")
                    {
                        if (!Messages.ContainsKey(message.MsgID))
                        {
                            Messages.Add(message.MsgID, message);
                        }
                        if (!MessagesByAutoID.ContainsKey(message.AutoID))
                        {
                            MessagesByAutoID.Add(message.AutoID, message);
                        }
                    }
                    message = new Message();
                    continue;
                }
                if (line.StartsWith("#:") && LoadComments)
                {
                    // context
                    message.Contexts.Add(line.Replace("#: ", ""));
                }
                if (line.StartsWith("#") && LoadComments)
                {
                    // comment
                    message.Comments.Add(line);
                    continue;
                }
                if (line.StartsWith("msgid"))
                {
                    // text we find from here out is part of the MessageID
                    parsingMsgID = true;
                }
                else if (line.StartsWith("msgstr"))
                {
                    // text we find from here on out is part of the Message String
                    parsingMsgID = false;
                }

                Match m = re.Match(line);
                if (m.Success)
                {
                    string token = m.Groups[1].Value;

                    if (parsingMsgID)
                    {
                        message.MsgID += token;
                    }
                    else
                    {
                        if (!line.StartsWith("msgstr"))
                        {
                            message.MsgStr += Environment.NewLine;
                        }
                        message.MsgStr += token;
                    }
                }
            }

            // put away the last one
            if (message.MsgID != "")
            {
                Messages.Add(message.MsgID, message);
            }

            reader.Close();

        }

        /// <summary>
        /// Given a msgID, returns the associated msgStr.
        /// If msgID doesn't exist in this collection, returns msgID.
        /// If the associated msgStr is empty, returns msgID.
        /// </summary>
        /// <param name="msgID"></param>
        /// <returns></returns>
        public string GetMessage(string msgID)
        {
            if (!Messages.ContainsKey(msgID))
            {
                return msgID;
            }

            return String.IsNullOrEmpty(Messages[msgID].MsgStr) ? msgID : Messages[msgID].MsgStr;
        }

        /// <summary>
        /// Writes the contents of this Localization to the specified .po file.
        /// </summary>
        /// <param name="filepath"></param>
        public void ToFile(string filepath)
        {
            using (StreamWriter writer = new StreamWriter(filepath, false))
            {
                foreach (Message msg in Messages.Values)
                {
                    writer.Write(msg.ToPOBlock());
                }
                writer.Close();
            }

        }
    }
}
