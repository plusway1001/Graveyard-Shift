GRAVEYARD SHIFT - Spooky Menu Kit
Drop the GraveyardShift_SpookyMenu folder into Assets/.

FONTS (make TMP Font Assets via Window > TextMeshPro > Font Asset Creator)
  Creepster      - big title / screen headers
  Patrick Hand   - chalk menu items, sliders, tips
  Luckiest Guy   - wooden plank button labels (BACK, OPTIONS...)

MAIN MENU (Canvas: Scale With Screen Size, 1920x1080)
  BG_MainMenu (full stretch, already vignetted) -> Title_Sign top-left
  Chalkboard_Menu (Image Type: Sliced) on the right
  Items: Start Shift / How to Play / Options / Credits / Clock Out
     each = Button + TMP label (Patrick Hand 68, colour #D0D6C8) + SpookyMenuButton
     children: Chalk_Highlight + Skull_Pointer (disabled)
  Receipt bottom-right with version + team.
  Tip: drop your seated ghost / table / chair / reaper sprites in front of the background for the live scene.

OPTIONS / HOW TO PLAY: dark overlay (black, alpha 0.6) + Chalkboard_Panel (Sliced).
  Slider: Background = Slider_Track_Chalk, Fill = Slider_Fill_Chalk, Handle = Slider_Knob_Bone
  Toggle: Background = Toggle_Box_Chalk, Checkmark = Toggle_Check_Chalk
  Back = Btn_Plank_Normal (Button Transition: Sprite Swap -> Hover / Pressed)

PAUSE: overlay + Chalkboard_Menu, 3 planks: Back to Work / Options / Clock Out
LOADING: BG + Vignette, walking reaper, chalk slider as progress bar, random tip.

Colours: chalk #D0D6C8, ghost green #96F5BE, blood red #BE1E28, wood #402E26
