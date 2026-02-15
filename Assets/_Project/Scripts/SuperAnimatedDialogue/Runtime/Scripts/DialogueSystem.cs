using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace SuperAnimatedDialogue.Runtime
{
    public class DialogueSystem : MonoBehaviour
    {
        private TextAnimationController textAnimationController;

        public bool IsActive { get; private set; } = false;

        [Header("UI")]
        public GameObject panel;
        public TextMeshProUGUI dialogueText;

        [Tooltip("Optional. If not set, choices will not be displayed.")]
        public Transform choicesParent;

        [Tooltip("Optional. If not set, choices will not be displayed.")]
        public GameObject choiceButtonPrefab;

        [Header("Dialogue")]
        public int startNode = 0;
        public List<DialogueNode> nodes = new List<DialogueNode>();

        private int currentNode = -1;
        private bool waitingForKey = false;

        private List<Button> currentButtons = new List<Button>();
        private int selectedIndex = 0;

        private bool blockContinueThisFrame = false;

        [NonSerialized] public bool isEndDialogue;

        [Serializable]
        public class DialogueNode
        {
            [TextArea(2, 4)]
            public string text;

            public int autoNextNode = -1;

            public List<DialogueChoice> choices = new List<DialogueChoice>();
        }

        [Serializable]
        public class DialogueChoice
        {
            public string text;
            public int nextNode = -1;
            public UnityEvent onChoose;
        }

        private void Awake()
        {
            textAnimationController = dialogueText.GetComponent<TextAnimationController>();

            if (textAnimationController == null)
            {
                Debug.LogWarning(
                    "TextAnimationController component not found on dialogueText. Text animation disabled."
                );
            }
        }

        private void Start()
        {
            StartDialogue();
        }

        public void StartDialogue()
        {
            if (IsActive) return;

            IsActive = true;
            ShowNode(startNode);
        }

        private void Update()
        {
            if (!IsActive)
                return;

            if (blockContinueThisFrame)
            {
                blockContinueThisFrame = false;
                return;
            }

            // =========================
            // CONTINUE (NO CHOICES)
            // =========================
            if (waitingForKey)
            {
                bool keyboard =
                    Keyboard.current != null &&
                    Keyboard.current.spaceKey.wasPressedThisFrame;

                bool controller =
                    Gamepad.current != null &&
                    Gamepad.current.buttonSouth.wasPressedThisFrame;

                if (keyboard || controller)
                {
                    waitingForKey = false;

                    var node = nodes[currentNode];

                    if (node.autoNextNode >= 0)
                        ShowNode(node.autoNextNode);
                    else
                        EndDialogue();

                    return;
                }
            }

            // =========================
            // CHOICES NAVIGATION
            // =========================
            if (currentButtons.Count > 0)
            {
                float axis = 0f;

                // Gamepad
                if (Gamepad.current != null)
                {
                    axis = Gamepad.current.leftStick.y.ReadValue();
                }

                // Keyboard
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                        axis = 1f;
                    if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                        axis = -1f;
                }

                if (axis > 0.5f)
                    MoveSelection(-1);
                else if (axis < -0.5f)
                    MoveSelection(1);

                bool keyboardConfirm =
                    Keyboard.current != null &&
                    Keyboard.current.enterKey.wasPressedThisFrame;

                bool controllerConfirm =
                    Gamepad.current != null &&
                    Gamepad.current.buttonSouth.wasPressedThisFrame;

                if (keyboardConfirm || controllerConfirm)
                {
                    currentButtons[selectedIndex].onClick.Invoke();
                }
            }
        }

        private void MoveSelection(int direction)
        {
            selectedIndex = Mathf.Clamp(
                selectedIndex + direction,
                0,
                currentButtons.Count - 1
            );

            currentButtons[selectedIndex].Select();
        }

        public void ShowNode(int index)
        {
            blockContinueThisFrame = true;
            IsActive = true;
            currentNode = index;

            if (index < 0 || index >= nodes.Count)
            {
                EndDialogue();
                return;
            }

            var node = nodes[index];

            if (textAnimationController != null)
                textAnimationController.SetText(node.text);
            else
                dialogueText.text = node.text;

            bool noChoices = node.choices == null || node.choices.Count == 0;

            if (choicesParent != null)
            {
                foreach (Transform child in choicesParent)
                    Destroy(child.gameObject);
            }

            currentButtons.Clear();
            selectedIndex = 0;

            if (noChoices)
            {
                waitingForKey = true;
                return;
            }

            waitingForKey = false;

            bool canShowChoices =
                choicesParent != null && choiceButtonPrefab != null;

            if (canShowChoices)
            {
                foreach (var choice in node.choices)
                {
                    GameObject btnObj =
                        Instantiate(choiceButtonPrefab, choicesParent);

                    var btnTxt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                    var btn = btnObj.GetComponent<Button>();

                    btnTxt.text = choice.text;

                    btn.onClick.AddListener(() =>
                    {
                        choice.onChoose?.Invoke();

                        if (choice.nextNode >= 0)
                            ShowNode(choice.nextNode);
                        else
                            EndDialogue();
                    });

                    currentButtons.Add(btn);
                }

                if (currentButtons.Count > 0)
                    currentButtons[0].Select();
            }
            else
            {
                waitingForKey = true;
            }
        }

        private void EndDialogue()
        {
            if (dialogueText != null)
                dialogueText.text = string.Empty;

            if (choicesParent != null)
            {
                foreach (Transform child in choicesParent)
                    Destroy(child.gameObject);
            }

            currentButtons.Clear();
            waitingForKey = false;
            IsActive = false;
            panel.SetActive(false);
            isEndDialogue = true;
        }
    }
}
