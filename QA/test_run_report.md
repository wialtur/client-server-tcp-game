# Test Run Report — Client-Server TCP Game

## Test Run Summary

**Environment:** Windows desktop application, two-player local/LAN test setup  
**Test Type:** Manual functional, negative, network, UI, and database testing  

## Scope

This test run covered the main two-player flow of the TCP client-server game, including joining a host session, color selection, ready-state behavior, game start, timer-based game ending, disconnect behavior, post-game results, score display, rating update, and rating persistence.

## Results

| Result | Count |
|---|---:|
| Passed | 14 |
| Failed | 4 |
| Blocked | 0 |
| Skipped | 0 |
| Total Executed | 18 |

## Passed Test Cases

| Test Case | Title |
|---|---|
| TC-001 | Second player joins existing host by IP |
| TC-002 | Joining player reaches color selection screen after joining host |
| TC-003 | Player cannot start game with default color |
| TC-004 | Players try to start game with the same selected color |
| TC-005 | Game starts after both players select different colors |
| TC-006 | Game does not start until both players click Ready |
| TC-009 | Player starts host session with empty name field |
| TC-011 | Joining player disconnects during color selection |
| TC-013 | Joining player disconnects during active game |
| TC-014 | Game ends when timer reaches zero |
| TC-015 | Player score is displayed on post-game screen |
| TC-016 | Player rating updates after normal game ends |
| TC-017 | Rating is saved after application restart |
| TC-018 | Rating is not updated after disconnect game ends |

## Failed Test Cases

| Test Case | Title | Bug |
|---|---|---|
| TC-007 | Player enters invalid IP address when joining | BUG-001 |
| TC-008 | Player tries to join when no host session exists | BUG-002 |
| TC-010 | Host disconnects during color selection | BUG-003 |
| TC-012 | Host disconnects during active game | BUG-004 |

## Defects Found

| Bug ID | Title | Severity | Priority | Status |
|---|---|---|---|---|
| BUG-001 | Application crashes after joining with invalid IP address | High | High | Open |
| BUG-002 | Application crashes when joining without active host session | High | High | Open |
| BUG-003 | Player 2 application crashes when host disconnects during color selection | High | High | Open |
| BUG-004 | Player 2 gets stuck when host disconnects during active game | High | Medium| Open |

## Summary

The core two-player happy path works successfully. Players can join the host session, reach the color selection screen, select colors, click **Ready**, start the game, finish the game by timer, view final scores, show rating, and keep rating data after restarting the application.

The main failures are related to connection validation and host disconnect handling. The application crashes when joining with an invalid IP address, crashes when joining without an active host session, crashes on Player 2 when the host disconnects during color selection, and leaves Player 2 stuck when the host disconnects during an active game.


