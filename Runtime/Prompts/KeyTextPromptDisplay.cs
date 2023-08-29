using qASIC.Input.Map;
using UnityEngine;
using qASIC.Input.Players;
using qASIC.Input.Devices;
using System.Linq;

namespace qASIC.Input.Prompts
{
    //TODO: update text when player or prompt index changes
    //TODO: make prompt index do something
    //TODO: add prompt index animation
    [AddComponentMenu("qASIC/Input/Prompts/Key Text Prompt Display")]
    public class KeyTextPromptDisplay : MonoBehaviour
    {
        public PromptLibrary library;
        public InputMapItemReference item;

        [Header("Type")]
        public int playerIndex;
        public int promptIndex;

        [Header("Text")]
        public TMPro.TMP_Text text;
        public string textFormat = "Press [{0}] to continue";
        public string notFoundText = "Unknown";

        private void OnValidate()
        {
            UpdatePrompt();
        }

        private void OnEnable()
        {
            UpdatePrompt();
            InputPlayerManager.OnPlayerCreated += InputPlayerManager_OnPlayerCreated;
            InputPlayerManager.OnPlayerRemoved += InputPlayerManager_OnPlayerRemoved;
        }

        private void OnDisable()
        {
            InputPlayerManager.OnPlayerCreated -= InputPlayerManager_OnPlayerCreated;
            InputPlayerManager.OnPlayerRemoved -= InputPlayerManager_OnPlayerRemoved;
        }

        private void InputPlayerManager_OnPlayerRemoved(InputPlayer player)
        {
            //Check for correct player index
            if (InputPlayerManager.Players.IndexOf(player) != playerIndex)
                return;

            player.OnLastDeviceChanged -= Player_OnLastDeviceChanged;
        } 

        private void InputPlayerManager_OnPlayerCreated(InputPlayer player)
        {
            //Check for correct player index
            if (InputPlayerManager.Players.IndexOf(player) != playerIndex)
                return;

            player.OnLastDeviceChanged += Player_OnLastDeviceChanged;
        }

        private void Player_OnLastDeviceChanged(IInputDevice device)
        {
            UpdatePrompt();
        }

        void UpdatePrompt()
        {
            if (library == null)
                return;

            var promptText = notFoundText;
            var mapItem = item.GetItem();

            if (mapItem != null &&
                mapItem is ISupportsPrompts promptItem)
            {
                var isPlayerConnected = InputPlayerManager.Players.IndexInRange(playerIndex) &&
                    InputPlayerManager.Players[playerIndex] != null;

                if (isPlayerConnected)
                {
                    var device = InputManager.Players[playerIndex].LastDevice ?? InputManager.Players[playerIndex].CurrentDevice;

                    var promptData = promptItem.GetPromptData();
                    var promptGroups = promptData
                        .promptGroups
                        .Where(x => x.keyPaths.Count > 0)
                        .Where(x => device.KeyRoots.Contains(x.keyPaths[0].Split('/').First()))
                        .ToArray();

                    if (promptGroups.Length > 0)
                    {
                        var index = Mathf.Clamp(promptIndex, 0, promptGroups.Length - 1);

                        var keys = library
                            .ForDevice(device)?
                            .GetPromptsFromPaths(promptGroups[index].keyPaths)
                            .ToDisplayNames();

                        promptText = promptItem.KeysToPromptText(keys);
                    }
                }
            }

            if (text != null)
                text.text = string.Format(textFormat, promptText);
        }
    }
}