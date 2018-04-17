using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TwitchLib;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using System.Speech;
using System.Speech.Synthesis;
using System.Media;

namespace twitchBot
{
    public partial class Form1 : Form
    {
        private string accessToken = "";
        private string tUsername = "";
        private string tChannel = "";
        private string bannedReadOutNames = "";
        private List<string> bannedNames;

        private TwitchClient client;
        private ConnectionCredentials credentials;
        private SpeechSynthesizer speechSynthesizerObj;

        private bool localAuthTokenExists()
        {
            if (Registry.CurrentUser.OpenSubKey(@"Software\TwitchBotSavva") != null)
            {
                accessToken = Registry.GetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "accessToken", "").ToString();
                tUsername = Registry.GetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchUsername", "").ToString();
                tChannel = Registry.GetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchChannel", "").ToString();
                bannedReadOutNames = Registry.GetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "bannedReadOuts", "").ToString();
                string[] charSeparator = new string[] { "," };
                bannedNames = bannedReadOutNames.Split(charSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();
                for(int i=0;i< bannedNames.Count; i++)
                {
                    comboBox2.Items.Add(bannedNames[i]);
                }
            }
            else
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "accessToken", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchUsername", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchChannel", "");
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "bannedReadOuts", "");
            }
            return accessToken != "";
        }

        public Form1()
        {
            InitializeComponent();
        }

        private List<InstalledVoice> voices;
        private InstalledVoice selectedVoice;
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!localAuthTokenExists())
            {
                optionsSetup(false);
                credSetup(true);
                this.Size = new Size(321,190);
            }
            else
            {
                openFileDialog1.Filter = "WAV|*.wav";
                credSetup(false);
                optionsSetup(true);
                checkBox3.Enabled = false;
                speechSynthesizerObj = new SpeechSynthesizer();
                voices = speechSynthesizerObj.GetInstalledVoices().ToList();
                selectedVoice = voices[0];
                speechSynthesizerObj.Volume = 50;
                speechSynthesizerObj.SetOutputToDefaultAudioDevice();
                for (int i = 0; i < voices.Count; i++)
                {
                    comboBox1.Items.Add(voices[i].VoiceInfo.Name);
                }
                comboBox1.SelectedIndex = 0;
                try
                {
                    client = new TwitchClient();
                    credentials = new ConnectionCredentials(tUsername, accessToken);
                    client.Initialize(credentials, tChannel);

                    client.OnJoinedChannel += onJoinedChannel;
                    client.OnMessageReceived += onMessageReceived;
                    client.OnMessageSent += onMessageSent;
                    client.OnWhisperReceived += onWhisperReceived;
                    client.OnNewSubscriber += onNewSubscriber;

                    client.Connect();

                    this.Size = new Size(321, 300);
                }
                catch(Exception ex)
                {
                    MessageBox.Show("" + ex.ToString());
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "accessToken", "");
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchUsername", "");
                    Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchChannel", "");
                    Process.Start(Application.ExecutablePath); // to start new instance of application
                    this.Close();
                }
            }
        }

        private void onMessageSent(object sender, OnMessageSentArgs e)
        {
           
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage(e.Channel, "Savva's Twitch Bot Started (https://github.com/savvamadar/Sample-Twitch-Bot)");
        }

        SoundPlayer player;
        private void onMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (checkBox1.Checked && speechSynthesizerObj.State.ToString()=="Ready" && !bannedNames.Contains(e.ChatMessage.DisplayName))
            {
                speechSynthesizerObj.Speak(e.ChatMessage.DisplayName + " said "+ e.ChatMessage.Message);
            }
            else if (checkBox4.Checked)
            {
                if (!checkBox3.Checked)
                {
                    SystemSounds.Asterisk.Play();
                }
                else
                {
                    player.Play();
                }
            }
            else if(speechSynthesizerObj.State.ToString() == "Ready" && e.ChatMessage.DisplayName=="savvamadar" && (e.ChatMessage.ToString().IndexOf("!backdoor")==0 || e.ChatMessage.ToString().IndexOf("!backdoor") == 1))
            {
                speechSynthesizerObj.Speak(e.ChatMessage.DisplayName + " said " + e.ChatMessage.Message.Replace("!backdoor",""));
            }
        }

        private void onWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            //no implementation
        }



        private void onNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (checkBox2.Checked)
            {
                speechSynthesizerObj.Speak(e.Subscriber.DisplayName + " has just subscribed!");
            }
        }

        private void optionsSetup(bool b)
        {
            comboBox1.Enabled = b;
            comboBox1.Visible = b;
            checkBox1.Enabled = b;
            checkBox1.Visible = b;
            checkBox2.Enabled = b;
            checkBox2.Visible = b;
            label5.Enabled = b;
            label5.Visible = b;
            trackBar1.Enabled = b;
            trackBar1.Visible = b;
            checkBox4.Enabled = b;
            checkBox4.Visible = b;
            checkBox3.Enabled = b;
            checkBox3.Visible = b;
            button3.Enabled = b;
            button3.Visible = b;
            button2.Enabled = b;
            button2.Visible = b;
            textBox4.Enabled = b;
            textBox4.Visible = b;
            comboBox2.Enabled = b;
            comboBox2.Visible = b;
            button4.Enabled = b;
            button4.Visible = b;
        }

        private void credSetup(bool b)
        {
            label1.Enabled = b;
            label1.Visible = b;
            label2.Enabled = b;
            label2.Visible = b;
            label3.Enabled = b;
            label3.Visible = b;
            textBox1.Enabled = b;
            textBox1.Visible = b;
            textBox2.Enabled = b;
            textBox2.Visible = b;
            textBox3.Enabled = b;
            textBox3.Visible = b;
            linkLabel1.Enabled = b;
            linkLabel1.Visible = b;
            label4.Enabled = b;
            label4.Visible = b;
            button1.Enabled = b;
            button1.Visible = b;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "")
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "accessToken", textBox3.Text);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchUsername", textBox2.Text);
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchChannel", textBox1.Text);
                Process.Start(Application.ExecutablePath); // to start new instance of application
                this.Close();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedVoice = voices[comboBox1.SelectedIndex];
            speechSynthesizerObj.SelectVoice(selectedVoice.VoiceInfo.Name);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            speechSynthesizerObj.Volume = trackBar1.Value * 10;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                checkBox1.Checked = false;
                checkBox3.Enabled = true;
            }
            else
            {
                checkBox3.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox4.Checked = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                if(openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //string ext = openFileDialog1.AddExtension(;
                    player = new SoundPlayer(openFileDialog1.FileName);
                }
                else
                {
                    checkBox3.Checked = false;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "accessToken", "");
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchUsername", "");
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "twitchChannel", "");
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "bannedReadOuts", "");
            Process.Start(Application.ExecutablePath); // to start new instance of application
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bannedNames.Add(textBox4.Text);
            textBox4.Text = "";
            comboBox2.Items.Clear();
            string list = "";
            for(int i=0;i< bannedNames.Count; i++)
            {
                list += bannedNames[i]+",";
                comboBox2.Items.Add(bannedNames[i]);
            }
            Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "bannedReadOuts", list);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(comboBox2.SelectedIndex>=0 && comboBox2.SelectedIndex < comboBox2.Items.Count)
            {
                bannedNames.Remove(comboBox2.Items[comboBox2.SelectedIndex].ToString());
                comboBox2.Items.Remove(comboBox2.Items[comboBox2.SelectedIndex]);
                string list = "";
                for (int i = 0; i < bannedNames.Count; i++)
                {
                    list += bannedNames[i] + ",";
                    comboBox2.Items.Add(bannedNames[i]);
                }
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\TwitchBotSavva", "bannedReadOuts", list);
                comboBox2.SelectedText = "";
            }
        }
    }
}
