# Test Plan — Client-Server TCP Game

## Project Overview

This project is a C# WPF client-server multiplayer game that uses TCP sockets for communication, JSON for message exchange, and SQLite for storing player ratings.

## Testing Objective

The goal of testing is to verify that the application works correctly in multiplayer mode, handles network communication reliably, updates game state between players, and saves rating data correctly.

## Scope of Testing

### In Scope

- Host creates a game session
- Client connects to host
- Lobby synchronization
- Player color selection
- Ready state handling
- Game start logic
- Real-time score updates
- Rating update after game
- SQLite data persistence
- Disconnect handling
- Invalid connection scenarios

### Out of Scope

- Performance testing 
- Security penetration testing
- Cross-platform testing outside Windows

## Test Types

- Functional testing
- UI testing
- Negative testing
- Regression testing
- Basic database testing
- Network interruption testing

## Test Environment

- OS: Windows 10
- Application type: WPF desktop application
- Build tested: GitHub release build
- Data storage: SQLite database used by the application
- Network: Localhost, Hamachi VPN, Radmin VPN
- Testing approach: Manual functional testing, negative testing, basic network testing, and database verification

## Risks

- Client and server may lose synchronization
- Application may not handle disconnects correctly
- Invalid network input may cause crashes
- Rating may not save correctly after a match
- UI may not update after receiving server messages

## Entry Criteria

- The application build is available
- The application launches successfully on Windows 10
- The main menu or start screen is accessible
- A local network connection is successfully established

## Exit Criteria

- All planned high-priority test cases have been executed
- Core multiplayer flow works from connection to game completion
- Rating data is saved correctly
- No critical or high-severity bugs remain open
- Failed test cases are documented with bug reports
- Test run results are recorded in the test run report