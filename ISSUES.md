# Diplomacy: Issues

Much of this issue context from DiplomacyFixes origins is thanks to Jiros, the fellow that'd been keeping the mod on life support for some time.


## From DiplomacyFixes: Specific Issues

- Incompatibility with other mods that do not use UIExtenderEx to modify the Encyclopedia Hero page (e.g., Extended Family)

- Alliances are broken (must investigate to observe specifics of exactly what this means)

- Game currently crashes when opening the Kingdom screen in BL e1.5.6

- "Messenger fix I had to add to fix the crashbug when a messenger is sent to a hero who then ends up in your party before the messenger arrives."

- "Expansionism should be reviewed, as should the corruption mechanic."

  + Expansionism was never really tested thoroughly by Katarn before being added to DiplomacyFixes; this part's no surprise.

- "Messenger should be reviewed overall, sometimes it's greyed out because the hero is somehow unavailable, but sometimes it's also because the hero is bugged out. I wrote a tweak in BT to 'fix' those stuck heroes, but here's an opportunity to resolve a game issues."

- "Should review war reparations and tribute mechanic. I noticed that once I bent a couple kingdoms to my will (beating them down to where they wanted to pay me like 1.5-2k/day to leave them alone), the game got ridiculously easy."

- "Usurp feature prob needs a review. I know we discussed the mutual hatred of the mechanic, lol."

  + The 'Usurp Throne' mechanic is garbage (something way more sophisticated -- something eventually in the works -- is required to make such a feature actually balanced and fun)

- "Min war duration may not be set up right. There was a report of it not working, but I never really experienced it."

- "War exhaustion seems to be ok, but prob want to look at the logic behind it to balance it out. It seems to diminish over time, but I could start a war w/ their war exhaustion moderately high and with a couple of strategic victories push them back to exhausted."

  + "Perhaps if you renew a war, they get pissed and have a huge boost to exhaustion (reset it to 0 or at least reduced dramatically). Maybe even reduce the rate at which it accumulates when you're 'picking' on a kingdom too much."

    - I can see some reasonably justified simulation alterations which would make 'picked on' kingdoms more resilient to war exhaustion.


## From DiplomacyFixes: Necessary Efforts

- We need to be using UIExtenderEx for all of our GUI modifications for maximum compatibility with vanilla changes and other mods' changes to the GUI.

- Need to transition to a fully nullable-aware codebase; the old code is littered with several hundred nullable warnings on C# 9 right now.