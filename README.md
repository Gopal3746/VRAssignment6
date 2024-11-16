# COMP 590-172 A6 - Escape Room

Demo Video: [YouTube](https://google.com)

Play the game: [Releases](https://github.com/Gopal3746/VRAssignment6/releases/tag/v0.4) (download and install apk onto Meta Quest)

![Screenshot of game](/screenshot.png)

## Game background

You find yourself trapped in an unfamiliar room filled with strange posters and puzzles. Can you escape in time to win?

## Project information

**Virtual Environment**: features two rooms and an open outside area if you are able to win.

**Puzzles**: features three puzzles that you must complete within 10 minutes.

<details>
  <summary>(Spoilers) Puzzle information</summary>

  The three puzzles consist of:

  1. **Color theory / matching puzzle**. A color wheel is provided as a poster on the wall, and you must match the correct colored vase onto the empty pedestal which reveals a hint for the next puzzle.
  2. **Riddle / secret code puzzle.** A hint revealed in the previous puzzle, paired with the *1984*-themed posters, help you figure out the 4 digit answer to this puzzle.
  3. **Physics puzzle**. You must switch the platforms to the correct orientation and press the red button in order for a ball to fall all the way to the green platform.

</details>

**Definition of a game**: This meets the definition of a game:

- *Fixed rules*: There are certain actions you may take (such as grabbing items, tapping objects, movement via teleporting) and other actions you cannot take.
- *Clear outcome*: You either win by escaping in time or lose by letting the timer run to zero.
- *Valorization of outcomes*: Winning by escaping is obviously better than losing, which is shown when you either collect your prizes (win) or the lights go red and an alarm sounds (lose).
- *Effort*: Effort and critical thinking is required to piece together clues in order to win.
- *Negotiable consequences*: There are no direct consequences to winning/losing.

**Movement**: Teleportation implemented by moving the thumb stick forward and pointing to your desired destination. You can also move a little by moving your head (for example, to bend down and grab a pot or get another perspective).

**Presence**: The game provides haptic and audio feedback. Controller vibrations are played whenever you interact with an "interactible" object such as by picking it up or pressing a button. An associated sound may also be played. Audio is used when the player sucessfully opens the door, wins, or loses.

**Documentation**: See demo video above, which gives an overview of the game, reflects on some of the design decisions, and (spoilers) shows a walkthrough.

## Controls

- Thumb stick forward: Teleport to pointed location
- Thumb stick left/right: Jump rotate to left/right a little
- Index (rear) trigger: Grab or tap object