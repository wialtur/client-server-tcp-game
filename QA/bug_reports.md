## BUG-001 — Application crashes after joining with invalid IP address

**Status:** Open  
**Severity:** High  
**Priority:** High  
**Environment:** Windows, desktop application  

### Preconditions

- Application is launched.

### Steps to Reproduce

1. Enter a player name.
2. Enter an invalid IP address.
3. Click **Join**.

### Actual Result

The application crashes.

### Expected Result

The application should not crash.  
The player should remain on the start screen or see a clear validation error.

## BUG-002 — Application crashes when joining without active host session

**Status:** Open  
**Severity:** High  
**Priority:** High  
**Environment:** Windows, desktop application  

### Preconditions

- No active host session exists for the entered IP address.

### Steps to Reproduce

1. Launch the application.
2. Enter a player name.
3. Enter a valid IP address where no host session is running.
4. Click **Join**.

### Actual Result

The application crashes.

### Expected Result

The application should not crash.  
The player should stay on the start screen or see a clear connection error.

## BUG-003 — Player 2 application crashes when host disconnects during color selection

**Status:** Open  
**Severity:** High  
**Priority:** High  
**Environment:** Windows, desktop application  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players are on the color selection screen.

### Steps to Reproduce

1. Player 1 starts a host session.
2. Player 2 joins using the host IP address.
3. Both players reach the color selection screen.
4. Player 1 closes the application.
5. Observe Player 2 application.

### Actual Result

Player 2 application crashes.

### Expected Result

Player 2 application should not crash.  
Player 2 should see a connection lost message or remain in a safe state.

## BUG-004 — Player 2 gets stuck when host disconnects during active game

**Status:** Open  
**Severity:** High  
**Priority:** Medium
**Environment:** Windows, desktop application  

### Preconditions

- Player 1 and Player 2 are connected to the same game session.
- Both players selected colors.
- Both players clicked **Ready**.
- The game has started.

### Steps to Reproduce

1. Player 1 starts a host session.
2. Player 2 joins using the host IP address.
3. Both players select colors.
4. Both players click **Ready**.
5. Game starts.
6. Player 1 closes the application during the active game.
7. Observe Player 2 application.

### Actual Result

Player 2 application does not crash, but the game stops working correctly.  
The balls stop falling, the timer continues until 0, and Player 2 remains stuck on the game screen.

### Expected Result

Player 2 should not remain stuck after the host disconnects.  
The application should show a connection lost message, end the game safely, or return Player 2 to a safe screen.

