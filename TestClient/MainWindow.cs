﻿using System;
using System.Windows.Forms;
using OBSWebsocketDotNet;

namespace TestClient
{
    public partial class MainWindow : Form
    {
        protected OBSWebsocket _obs;

        public MainWindow()
        {
            InitializeComponent();
            _obs = new OBSWebsocket();

            _obs.OnSceneChange += onSceneChange;
            _obs.OnSceneCollectionChange += onSceneColChange;
            _obs.OnProfileChange += onProfileChange;
            _obs.OnTransitionChange += onTransitionChange;
            _obs.OnTransitionDurationChange += onTransitionDurationChange;

            _obs.OnStreamingStateChange += onStreamingStateChange;
            _obs.OnRecordingStateChange += onRecordingStateChange;

            _obs.OnStreamStatus += onStreamData;
        }

        private void onSceneChange(OBSWebsocket sender, string newSceneName)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbCurrentScene.Text = newSceneName;
            });
        }

        private void onSceneColChange(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbSceneCol.Text = _obs.GetCurrentSceneCollection();
            });
        }

        private void onProfileChange(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbProfile.Text = _obs.GetCurrentProfile();
            });
        }

        private void onTransitionChange(OBSWebsocket sender, string newTransitionName)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbTransition.Text = newTransitionName;
            });
        }

        private void onTransitionDurationChange(OBSWebsocket sender, int newDuration)
        {
            tbTransitionDuration.Value = newDuration;
        }

        private void onStreamingStateChange(OBSWebsocket sender, OutputStateUpdate newState)
        {
            string state = "";
            switch(newState)
            {
                case OutputStateUpdate.Starting:
                    state = "Stream starting...";
                    break;

                case OutputStateUpdate.Started:
                    state = "Stop streaming";
                    BeginInvoke((MethodInvoker)delegate
                    {
                        gbStatus.Enabled = true;
                    });
                    break;

                case OutputStateUpdate.Stopping:
                    state = "Stream stopping...";
                    break;

                case OutputStateUpdate.Stopped:
                    state = "Start streaming";
                    BeginInvoke((MethodInvoker)delegate
                    {
                        gbStatus.Enabled = false;
                    });
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            BeginInvoke((MethodInvoker)delegate
            {
                btnToggleStreaming.Text = state;
            });
        }

        private void onRecordingStateChange(OBSWebsocket sender, OutputStateUpdate newState)
        {
            string state = "";
            switch (newState)
            {
                case OutputStateUpdate.Starting:
                    state = "Recording starting...";
                    break;

                case OutputStateUpdate.Started:
                    state = "Stop recording";
                    break;

                case OutputStateUpdate.Stopping:
                    state = "Recording stopping...";
                    break;

                case OutputStateUpdate.Stopped:
                    state = "Start recording";
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            BeginInvoke((MethodInvoker)delegate
            {
                btnToggleRecording.Text = state;
            });
        }

        private void onStreamData(OBSWebsocket sender, OBSStreamStatus data)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                txtStreamTime.Text = data.TotalStreamTime.ToString() + " sec";
                txtKbitsSec.Text = data.KbitsPerSec.ToString() + " kbit/s";
                txtBytesSec.Text = data.BytesPerSec.ToString() + " bytes/s";
                txtFramerate.Text = data.FPS.ToString() + " FPS";
                txtStrain.Text = (data.Strain * 100).ToString() + " %";
                txtDroppedFrames.Text = data.DroppedFrames.ToString();
                txtTotalFrames.Text = data.TotalFrames.ToString();
            });
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                _obs.Connect(txtServerIP.Text, txtServerPassword.Text);
                btnConnect.Enabled = false;
                txtServerIP.Enabled = false;
                txtServerPassword.Enabled = false;
                gbControls.Enabled = true;

                var versionInfo = _obs.GetVersion();
                tbPluginVersion.Text = versionInfo.PluginVersion;
                tbAPIVersion.Text = versionInfo.APIVersion;
                tbOBSVersion.Text = versionInfo.OBSStudioVersion;

                btnListScenes.PerformClick();
                btnGetCurrentScene.PerformClick();

                btnListSceneCol.PerformClick(); 
                btnGetCurrentSceneCol.PerformClick();

                btnListProfiles.PerformClick();
                btnGetCurrentProfile.PerformClick();

                btnListTransitions.PerformClick();
                btnGetCurrentTransition.PerformClick();

                btnGetTransitionDuration.PerformClick();

                var streamStatus = _obs.GetStreamingStatus();
                if (streamStatus.IsStreaming)
                    onStreamingStateChange(_obs, OutputStateUpdate.Started);
                else
                    onStreamingStateChange(_obs, OutputStateUpdate.Stopped);

                if (streamStatus.IsRecording)
                    onRecordingStateChange(_obs, OutputStateUpdate.Started);
                else
                    onRecordingStateChange(_obs, OutputStateUpdate.Stopped);
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show("Connect failed : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnListScenes_Click(object sender, EventArgs e)
        {
            var scenes = _obs.ListScenes();

            tvScenes.Nodes.Clear();
            foreach(var scene in scenes)
            {
                var node = new TreeNode(scene.Name);
                foreach (var item in scene.Items)
                {
                    node.Nodes.Add(item.Name);
                }

                tvScenes.Nodes.Add(node);
            }
        }

        private void btnGetCurrentScene_Click(object sender, EventArgs e)
        {
            tbCurrentScene.Text = _obs.GetCurrentScene().Name;
        }

        private void btnSetCurrentScene_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentScene(tbCurrentScene.Text);
        }

        private void tvScenes_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbCurrentScene.Text = e.Node.Text;
            }
        }

        private void btnListSceneCol_Click(object sender, EventArgs e)
        {
            var sc = _obs.ListSceneCollections();

            tvSceneCols.Nodes.Clear();
            foreach (var sceneCol in sc)
            {
                tvSceneCols.Nodes.Add(sceneCol);
            }
        }

        private void btnGetCurrentSceneCol_Click(object sender, EventArgs e)
        {
            tbSceneCol.Text = _obs.GetCurrentSceneCollection();
        }

        private void btnSetCurrentSceneCol_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentSceneCollection(tbSceneCol.Text);
        }

        private void tvSceneCols_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbSceneCol.Text = e.Node.Text;
            }
        }

        private void btnListProfiles_Click(object sender, EventArgs e)
        {
            var profiles = _obs.ListProfiles();

            tvProfiles.Nodes.Clear();
            foreach (var profile in profiles)
            {
                tvProfiles.Nodes.Add(profile);
            }
        }

        private void btnGetCurrentProfile_Click(object sender, EventArgs e)
        {
            tbProfile.Text = _obs.GetCurrentProfile();
        }

        private void btnSetCurrentProfile_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentProfile(tbProfile.Text);
        }

        private void tvProfiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbProfile.Text = e.Node.Text;
            }
        }

        private void btnToggleStreaming_Click(object sender, EventArgs e)
        {
            _obs.ToggleStreaming();
        }

        private void btnToggleRecording_Click(object sender, EventArgs e)
        {
            _obs.ToggleRecording();
        }

        private void btnListTransitions_Click(object sender, EventArgs e)
        {
            var transitions = _obs.ListTransitions();

            tvTransitions.Nodes.Clear();
            foreach (var transition in transitions)
            {
                tvTransitions.Nodes.Add(transition);
            }
        }

        private void btnGetCurrentTransition_Click(object sender, EventArgs e)
        {
            tbTransition.Text = _obs.GetCurrentTransition().Name;
        }

        private void btnSetCurrentTransition_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentTransition(tbTransition.Text);
        }

        private void tvTransitions_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbTransition.Text = e.Node.Text;
            }
        }

        private void btnGetTransitionDuration_Click(object sender, EventArgs e)
        {
            tbTransitionDuration.Value = _obs.GetCurrentTransition().Duration;
        }

        private void btnSetTransitionDuration_Click(object sender, EventArgs e)
        {
            _obs.SetTransitionDuration((int)tbTransitionDuration.Value);
        }
    }
}