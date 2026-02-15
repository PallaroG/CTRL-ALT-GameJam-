Text Animation Controller for Unity (TextMeshPro)
Overview
This asset provides a robust and flexible system for creating complex and dynamic text animations using TextMeshProUGUI in Unity. It allows you to apply multiple animation effects such as Wave, Shake, Bounce, Scale, Rotate, Jitter, Impact, Perspective, and Rainbow. You can also apply different animation profiles to specific parts of the same text using rich text tags.

Features
- Multiple Animation Profiles: Create and manage multiple animation profiles, each with its own combination of effects.
- Tag-Based Animation: Use [anim=ProfileName] and [/anim] tags to apply animation profiles to specific parts of your text.
- Live Preview in Inspector: Preview text animations in real time directly in the Inspector, without entering Play Mode.
- Typewriter Effect: Character-by-character text animation with optional sound support.
- Custom Editor: Clean and intuitive custom Inspector for managing animation profiles and global settings.

How to Use?
1. Add the TextAnimationController component to the same GameObject that contains your TextMeshProUGUI component.
2. In the Inspector, you will see the custom Text Animation Controller editor.

Configuring Animation Profiles
- Animation Profiles: This section allows you to create and manage your animation profiles.
- The Default profile (index 0) is the base profile applied to all text that is not inside an animation tag.
- Click "Add New Profile" to create additional profiles (for example: "WaveProfile", "ShakeProfile").
- For each profile, you can enable and configure the desired effects (Wave, Shake, Rainbow, etc.).

Using Animation Tags
To apply an animation profile to a specific portion of the text, use the [anim=ProfileName] and [/anim] tags.
Example:
This is default text. [anim=WaveProfile]This part will use the Wave animation.[/anim] And this part returns to default.

Live Preview
To preview animations without entering Play Mode:

- Live Preview Settings:
  - Preview Text: Enter the text you want to animate in the preview.
  - Live Preview: Enable this option to start real-time animation in the Inspector.
- In each Animation Profile:
- Click the button "Preview this Profile: [Profile Name]" to choose which profile is displayed in the preview area.

Script Integration
To update the text dynamically (for example, in a dialogue system), call the SetText(string newText) method from the TextAnimationController.
Example:

public TextAnimationController textAnimationController;

void Start()
{
    textAnimationController.SetText("This is a [anim=ShakeProfile]dynamic[/anim] text!");
}

This ensures that the new text is properly processed, including animation tags, and that the animation is restarted correctly.