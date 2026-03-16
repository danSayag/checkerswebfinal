# CheckersWeb ♟️

A **real-time multiplayer Checkers game** built with **ASP.NET Core MVC**, **SignalR**, and **SQLite**.

Players can create games, invite others via a link, and play checkers online with real-time updates.

---

## 🚀 Features

- 👤 User registration and login  
- 🎮 Create and join games via invite link  
- ⚡ Real-time gameplay using **SignalR**  
- ♟️ Full checkers rules:
  - captures
  - king promotion
  - king multi-square movement
- 🔄 Board flips perspective per player  
- 🟢 Valid move highlighting  
- 🏆 Win detection with popup  
- 📋 Open games lobby on the home page  
- 🥇 Leaderboard showing wins per player  

---

## 🛠 Tech Stack

| Layer | Technology |
|------|-------------|
| Backend | ASP.NET Core 9 MVC |
| Real-time communication | SignalR |
| Database | SQLite + Entity Framework Core |
| Frontend | Vanilla JavaScript |
| UI | Bootstrap 5 |
| Authentication | Cookie Authentication |

---

## 📂 Project Structure

```
CheckersWeb/
├── Controllers/
│   ├── HomeController.cs
│   ├── GameController.cs
│   └── AccountController.cs
│
├── Hubs/
│   └── GameHub.cs
│
├── Models/
│   ├── Game.cs
│   ├── Piece.cs
│   ├── User.cs
│
├── Services/
│   └── ConnectedUsers.cs
│
├── Data/
│   └── UserDbContext.cs
│
├── Views/
│   ├── Home/
│   └── Game/
│
└── wwwroot/
    ├── js/
    │   └── script.js
    ├── css/
    │   └── style.css
    └── images/
```

---

## ⚙️ Getting Started

### Prerequisites

Install:

- [.NET 9 SDK](https://dotnet.microsoft.com/download)

---

### Run the Project

```bash
git clone https://github.com/YOUR_USERNAME/CheckersWeb.git
cd CheckersWeb
dotnet run
```

The application will:

- Automatically apply **Entity Framework migrations**
- Create the **SQLite database (`users.db`)**

Open the browser:

```
https://localhost:7013
```

(The port number will appear in the terminal.)

---

## 🗄 Database

The project uses **SQLite**.

When the app runs for the first time:

```
users.db
```

will automatically be created.

---

### Reset the Database

1. Stop the application  
2. Delete:

```
CheckersWeb/users.db
```

3. Run the app again  

The database will be recreated automatically.

---

## 🎮 How to Play

1️⃣ **Create an account** and log in  

2️⃣ Click **Create Game**

3️⃣ Share the **invite link** with another player

4️⃣ When they join, the game starts automatically

5️⃣ Player roles:

| Player | Color |
|------|------|
| Player 1 | White |
| Player 2 | Black |

6️⃣ Click a piece to select it  

7️⃣ Valid moves appear in **green**

8️⃣ Click a highlighted square to move

9️⃣ Capture all opponent pieces to **win**

---

## ♟ Game Rules

- Regular pieces move **diagonally forward** one square
- Capture by **jumping over an opponent's piece**
- A capture moves **two squares diagonally**
- When a piece reaches the **opposite side**, it becomes a **King**

### King Rules

Kings can:

- Move diagonally in **all four directions**
- Move **multiple squares**
- Capture by jumping over one opponent piece

---

## 🔮 Possible Future Improvements

- Online matchmaking
- Chat during games
- Player ranking system
- Spectator mode
- Game history
- Mobile-friendly UI

---

## 👨‍💻 Authors

**Dan Sayag**  
Computer Science Student – Netanya Academic College
**Tomer Levi**  
Computer Science Student – Netanya Academic College
