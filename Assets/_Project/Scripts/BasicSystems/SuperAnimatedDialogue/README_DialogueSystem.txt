Simple Dialogue System for Unity
Overview
This is a simple yet robust dialogue system designed to be flexible and easy to integrate with other components, such as the TextAnimationController. It uses a Dialogue Node–based approach to create linear or choice-driven conversation flows.

Features
- Node-Based Dialogue Flow: Define the dialogue text, the next action (next node or end of dialogue), and choices for each node.
- Optional Choices: A node can contain multiple choices (buttons) or act as a simple continuation node (waiting for a key press).
- Integration with Text Animation Controller: The system is optimized to work seamlessly with the TextAnimationController for animated text.
- Custom Editor: Clean and intuitive Inspector for managing the node list and their properties.
- Automatic Start: The dialogue starts automatically when the GameObject containing the script becomes active in the scene.

How to Use
1. Create a GameObject in your scene (for example, "Dialogue Manager") and add the DialogueSystem component.
2. Create the required UI for the dialogue:
   - A TextMeshProUGUI component for the dialogue text.
   - A Transform to act as the parent for choice buttons.

Configuring the DialogueSystem
In the DialogueSystem Inspector:

UI Elements:
- Dialogue Text:
  Assign your TextMeshProUGUI component here.
- Choices Parent:
  Assign the Transform that will act as the parent for the choice buttons (optional).
- Choice Button Prefab:
  Assign the choice button prefab (must contain a Button component and a TextMeshProUGUI child) (optional).

General Settings:
- Continue Key: Key used to advance dialogue nodes without choices (default: Space).
- Start Node: Index of the node where the dialogue should begin (default: 0).

Dialogue Nodes:
- Use the array size control to add new dialogue nodes.
- For each node, define:
  - Text
  - Auto Next Node (for nodes without choices)
  - Choices (if applicable)

Integration with Text Animation Controller
If a TextAnimationController is present on the same GameObject as the TextMeshProUGUI (or on the DialogueSystem itself if the dialogueText is the TextMeshProUGUI), the DialogueSystem will detect it automatically.

When ShowNode is called, the DialogueSystem uses the SetText method from the TextAnimationController, ensuring that all rich animation tags work correctly for each line of dialogue.

Starting and Ending the Dialogue
Start:
- The dialogue starts automatically in Start() by calling StartDialogue().

End:
The dialogue ends when:
- A node points to an Auto Next Node value of -1.
- A choice points to a Next Node value of -1.
- The player presses the Exit Key.

You can also start the dialogue from another script:

public DialogueSystem dialogueSystem;
void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        dialogueSystem.StartDialogue();
    }
}