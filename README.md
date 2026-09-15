# ChatServer

Backend do ChatApp: uma API REST + hub SignalR em **ASP.NET Core 8**, com autenticação via **JWT** e persistência em **PostgreSQL** através do **Entity Framework Core 8**.

## Stack

- ASP.NET Core 8 (Web API)
- SignalR (mensagens e presença em tempo real)
- Entity Framework Core 8 + Npgsql (PostgreSQL)
- JWT Bearer Authentication (`System.IdentityModel.Tokens.Jwt`)
- `Microsoft.AspNetCore.Identity` (`PasswordHasher<T>` para hashing de senha)

## Estrutura do projeto

```
ChatServer/
├── Controllers/          # AuthController, ConversationController, UserController, MessagesController
├── Hubs/                 # ChatHub (SignalR)
├── Realtime/             # ChatNotifier (implementa IChatNotifier)
├── Services/             # AuthService, ConversationService, ChatService, UserService, TokenService, PasswordService, EmailValidator, InMemoryUserConnectionTracker
│   └── Interfaces/
├── Data/
│   ├── Models/            # User, Conversation, ConversationParticipant, Message, RefreshToken
│   ├── Repositories/       # UserRepository, ConversationRepository, MessageRepository, RefreshTokenRepository
│   │   └── Interfaces/
│   ├── ChatDbContext.cs
│   └── Migrations/
├── Contracts/             # DTOs (records): MessageDto, ConversationDto, ConversationSummaryDto, UserProfileDto, UserSummaryDto
├── DTO/
│   ├── Request/            # LoginRequest, SignUpRequest, CreateGroupConversationRequest
│   ├── Response/           # AuthResponse
│   └── Result/              # Result pattern: AuthResult, ConversationResult, MessagesQueryResult, SendMessageResult, MarkAsReadResult
├── Common/Enums/           # ConversationType, ParticipantRole, MessageStatus, UserStatus
├── appsettings.json
└── appsettings.Development.json

ChatServer.DTO/            # Projeto legado com DTOs em classes mutáveis (UserDTO, ChatDTO, MessageDTO, AttachDTO)
```

> `ChatServer.DTO` é um projeto separado e mais antigo, com DTOs que se sobrepõem aos `Contracts` do `ChatServer`. Mantido no repositório, mas não é a fonte de verdade atual para os contratos da API.

## Domínio

- **User**: username/email únicos, hash de senha, `SecurityStamp` (invalida tokens ao trocar senha), controle de tentativas de login (`AccessFailedCount`, `LockoutEndAt`).
- **Conversation**: `Direct` ou `Group`, com `ConversationParticipant` (chave composta `ConversationId + UserId`, papel `Member`/`Admin`, `LastReadAt`).
- **Message**: conteúdo (até 4000 caracteres), status (`Sent`/`Delivered`/`Read`), soft delete (`IsDeleted`).
- **RefreshToken**: nunca armazenado em texto puro — apenas o hash (`TokenHash`), com rotação e rastreamento de substituição (`ReplacedByTokenId`).

## Autenticação

- Login: bloqueio de conta após 5 tentativas inválidas (15 minutos de lockout).
- Access token JWT expira em 15 minutos; refresh token é rotacionado a cada uso.
- Reuso de um refresh token já revogado/expirado é tratado como possível comprometimento de sessão: todos os tokens ativos do usuário são revogados (`AuthResult.SessionCompromised`).
- Endpoints com `[EnableRateLimiting("auth")]` em login/refresh/signup.

## Endpoints principais

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/auth/login` | Autenticação, retorna access + refresh token |
| POST | `/api/auth/refresh` | Renova o par de tokens |
| POST | `/api/auth/logout` | Revoga o refresh token |
| POST | `/api/auth/signup` | Cria novo usuário |
| GET | `/api/conversation` | Lista conversas do usuário autenticado |
| POST | `/api/conversation/direct` | Cria/recupera conversa direta |
| POST | `/api/conversation/group` | Cria conversa em grupo (mínimo 3 participantes) |
| GET | `/api/conversation/{id}/messages` | Histórico paginado (cursor por `before`) |
| POST | `/api/conversation/{id}/participants` | Adiciona participante (somente Admin) |
| DELETE | `/api/conversation/{id}/participants/{userId}` | Remove participante (Admin ou o próprio usuário) |
| GET | `/api/user/me` | Perfil do usuário autenticado |
| GET | `/api/user/search?q=` | Busca usuários por username |
| PUT | `/api/user/me/avatar` | Atualiza avatar |

Mensagens em si (envio, marcação como lida, "digitando...") passam pelo **ChatHub** (SignalR), não por endpoints REST — `MessagesController` hoje só injeta `IChatService`, sem rotas implementadas.

## Hub SignalR — `ChatHub`

Métodos: `SendMessage`, `MarkAsRead`, `Typing`.
Eventos emitidos: `MessageReceived`, `MessageRead`, `Typing`, `UserStatusChanged`.
Rastreamento de conexões via `IUserConnectionTracker` (implementação atual: `InMemoryUserConnectionTracker`, em memória local ao processo).

## Configuração

`appsettings.Development.json` (exemplo, ajuste para seu ambiente):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Port=5432;Database=Chat;User Id=...;Password=..."
  },
  "Jwt": {
    "SigningKey": "<chave secreta>",
    "Issuer": "YourIssuerHere",
    "Audience": "YourAudienceHere"
  }
}
```

> Não use segredos reais commitados no `appsettings.Development.json` em produção — mova connection string e chave de assinatura JWT para variáveis de ambiente, User Secrets ou um cofre de segredos.

## Rodando localmente

```bash
dotnet restore
dotnet ef database update --project ChatServer
dotnet run --project ChatServer
```

A API sobe por padrão em `http://localhost:5051` (perfil `http` em `launchSettings.json`).

## Status conhecido / pendências

- `Program.cs` ainda está no template padrão — registro de EF Core, autenticação JWT, SignalR e DI dos serviços precisa ser conferido/completado.
- `ChatService.DeleteMessageAsync` não implementado (`NotImplementedException`).
- `ConversationType` ainda só tem `Direct`/`Group` — fluxo de convite/aprovação de conversa planejado, mas não implementado neste snapshot.
