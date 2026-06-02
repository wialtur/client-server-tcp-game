	
## TC-001 — Second player joins existing host by IP

**Priority:** High  
**Type:** Functional  

### Preconditions

- Player 1 has launched the application.
- Player 1 has clicked **Host**.
- Player 2 knows the host IP address.

### Steps

1. Launch the application as Player 2.
2. Enter the host IP address.
3. Click **Join**.

### Expected Result

Player 2 connects to the host session.  
Player 2 screen changes to the color selection screen.

### Status

Passed 

### Actual Result

Player 2 connected successfully and the screen changed to the color selection screen.


## TC-002 — Joining player reaches color selection screen after joining host

**Priority:** High  
**Type:** Functional  

### Preconditions

- Player 1 and Player 2 are using the same valid host IP address.

### Steps

1. Player 1 launches the application.
2. Player 2 launches the application.
3. Player 1 enters name and IP address.
4. Player 1 clicks **Host**.
5. Player 2 enters name and the same IP address.
6. Player 2 clicks **Join**.
7. Observe Player 2 screen.

### Expected Result

Player 2 screen changes to the color selection screen.

### Status

Passed

### Actual Result

Player 2 screen changed to the color selection screen.


## TC-003 — Player cannot start game with default color

**Priority:** High  
**Type:** Negative / Functional  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players are on the color selection screen.

### Steps

1. Player 1 clicks **Ready**.
2. Player 2 selects a color.
3. Player 2 clicks **Ready**.
4. Observe whether the game starts.

### Expected Result

The game does not start while Players were using the default color.

### Status

Passed

### Actual Result

The game did not start while Players were using the default color.

## TC-004 — Players try to start game with the same selected color

**Priority:** Medium  
**Type:** Functional  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players are on the color selection screen.

### Steps

1. Player 1 selects a color.
2. Player 2 selects the same color.
3. Player 1 clicks **Ready**.
4. Player 2 clicks **Ready**.
5. Observe whether the game starts.
6. Observe both players' colors after the game starts.

### Expected Result

The game starts successfully.  
One player keeps the selected color, and the other player is assigned the default color.

### Status

Passed

### Actual Result

The game started successfully.  
One player kept the selected color, and the other player was assigned the default color.

## TC-005 — Game starts after both players select different colors

**Priority:** High  
**Type:** Functional  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players are on the color selection screen.

### Steps

1. Player 1 selects a color.
2. Player 2 selects a different color.
3. Player 1 clicks **Ready**.
4. Player 2 clicks **Ready**.
5. Observe both players' screens.

### Expected Result

The game starts successfully after both players select different colors and click **Ready**.

### Status

Passed

### Actual Result

The game started successfully after both players selected different colors and clicked **Ready**.

## TC-006 — Game does not start until both players click Ready

**Priority:** High  
**Type:** Functional / Negative  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players are on the color selection screen.

### Steps

1. Player 1 selects a color.
2. Player 2 selects a different color.
3. Player 1 clicks **Ready**.
4. Player 2 does not click **Ready**.
5. Observe both players' screens.

### Expected Result

The game does not start until Player 2 also clicks **Ready**.

### Status

Passed

### Actual Result

The game did not start until both players clicked **Ready**.

## TC-007 — Player enters invalid IP address when joining

**Priority:** High  
**Type:** Negative / Validation  

### Preconditions

- Application is launched.

### Steps

1. Player enters a name.
2. Player enters an invalid IP address.
3. Player clicks **Join**.
4. Observe the application behavior.

### Expected Result

The application does not crash.  
The player does not reach the color selection screen.

### Status

Failed

### Actual Result

The application crashed after the player entered an invalid IP address and clicked **Join**.

### Bug

BUG-001

## TC-008 — Player tries to join when no host session exists

**Priority:** High  
**Type:** Negative / Network  

### Preconditions

- No player has clicked **Host**.
- No active host session exists for the entered IP address.

### Steps

1. Player launches the application.
2. Player enters a name.
3. Player enters a valid IP address where no host session is running.
4. Player clicks **Join**.
5. Observe the application behavior.

### Expected Result

The application does not crash.  
The player does not reach the color selection screen.

### Status

Failed

### Actual Result

The application crashed after the player tried to join when no host session existed.

### Bug

BUG-002

## TC-009 — Player starts host session with empty name field

**Priority:** Medium  
**Type:** Functional  

### Preconditions

- Application is launched.

### Steps

1. Leave the player name field empty.
2. Enter a valid IP address.
3. Click **Host**.
4. Connect Player 2 to the same host session.
5. Observe the color selection screen.

### Expected Result

The player is moved to the color selection screen.  
If no name is entered, the application assigns a default player name such as **P1** or **P2**.

### Status

Passed

### Actual Result

The player was moved to the color selection screen.  
The application assigned a default player name.

## TC-010 — Host disconnects during color selection

**Priority:** High  
**Type:** Negative / Network  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players are on the color selection screen.

### Steps

1. Player 1 closes the application.
2. Observe Player 2 screen.

### Expected Result

Player 2 application does not crash.  
Player 2 is returned to a safe state or remains on screen without freezing.

### Status

Failed

### Actual Result

Player 2 application crashed after Player 1 closed the application.

### Bug

BUG-003

## TC-011 — Joining player disconnects during color selection

**Priority:** Medium
**Type:** Negative / Network  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players are on the color selection screen.

### Steps

1. Player 2 closes the application.
2. Observe Player 1 screen.

### Expected Result

Player 1 application does not crash.  
Player 1 remains in a safe state after Player 2 disconnects.

### Status

Passed

### Actual Result

Player 1 application did not crash after Player 2 closed the application during color selection.

## TC-012 — Host disconnects during active game

**Priority:** High  
**Type:** Negative / Network  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players selected colors.
- Both players clicked **Ready**.
- The game has started.

### Steps

1. Player 1 closes the application during the active game.
2. Observe Player 2 application.

### Expected Result

Player 2 application does not crash.  
Player 2 remains in a safe state after the host disconnects.

### Status

Failed

### Actual Result

Player 2 application did not crash after Player 1 closed the application during the active game.  
However, the balls stopped falling, the timer continued until 0, and Player 2 remained stuck on the game screen.

### Bug

BUG-004

## TC-013 — Joining player disconnects during active game

**Priority:** High  
**Type:** Negative / Network  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players selected colors.
- Both players clicked **Ready**.
- The game has started.

### Steps

1. Player 2 closes the application during the active game.
2. Observe Player 1 application.

### Expected Result

Player 1 is moved to the post-game screen.  
The game end reason is shown as **DISCONNECT**.

### Status

Passed

### Actual Result

Player 1 was moved to the post-game screen after Player 2 disconnected during the active game.  
The game end reason was shown as **DISCONNECT**.

## TC-014 — Game ends when timer reaches zero

**Priority:** High  
**Type:** Functional  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players selected colors.
- Both players clicked **Ready**.
- The game has started.

### Steps

1. Do not disconnect either player.
2. Let the game timer run until it reaches zero.
3. Observe both players' screens.

### Expected Result

Both players are moved to the post-game screen.  
The game end reason is shown as **TIME**.

### Status

Passed

### Actual Result

Both players were moved to the post-game screen after the timer reached zero.  
The game end reason was shown as **TIME**.

## TC-015 — Player score is displayed on post-game screen

**Priority:** Medium
**Type:** Functional / UI  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players selected colors.
- Both players clicked **Ready**.
- The game has started.

### Steps

1. Play the game until the timer reaches zero.
2. Observe the post-game screen for both players.

### Expected Result

The post-game screen displays each player’s final score.

### Status

Passed

### Actual Result

The post-game screen displayed each player’s final score.

## TC-016 — Player rating updates after normal game ends

**Priority:** High  
**Type:** Functional / Database  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players selected colors.
- Both players clicked **Ready**.
- The game has started.
- The game ends normally when the timer reaches zero.

### Steps

1. Note both players' ratings before the game starts.
2. Play the game until the timer reaches zero.
3. On the post-game screen, click **Show Rating**.
4. Check both players' ratings.

### Expected Result

After clicking **Show Rating**, both players' updated ratings are displayed.

### Status

Passed

### Actual Result

After clicking **Show Rating**, both players' updated ratings were displayed.

## TC-017 — Rating is saved after application restart

**Priority:** High  
**Type:** Database / Persistence  

### Preconditions

- A previous game has ended normally with reason **TIME**.
- Ratings were updated after clicking **Show Rating**.

### Steps

1. Close the application for both players.
2. Launch the application again for both players.
3. Player 1 enters the same name and IP address.
4. Player 1 clicks **Host**.
5. Player 2 enters the same name and same IP address.
6. Player 2 clicks **Join**.
7. Both players select colors.
8. Both players click **Ready**.
9. Play the game until the timer reaches zero.
10. On the results screen, click **Show Rating**.

### Expected Result

The rating shown after restart includes previously saved rating data and is updated after the new completed game.

### Status

Passed

### Actual Result

After restarting the application and completing another game, the rating was displayed on the results screen and included previously saved rating data.

## TC-018 — Rating is not updated after disconnect game ends

**Priority:** Medium  
**Type:** Functional / Database  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players selected colors.
- Both players clicked **Ready**.
- The game has started.

### Steps

1. Player 2 closes the application during the active game.
2. Observe Player 1 results screen.
3. Click **Show Rating**.

### Expected Result

Player 1 is moved to the results screen.  
The game end reason is shown as **DISCONNECT**.  
Rating is not updated for a game that ended due to disconnect.

### Status

Passed

### Actual Result

Player 1 was moved to the results screen with reason **DISCONNECT**.  
After clicking **Show Rating**, rating was not updated.




