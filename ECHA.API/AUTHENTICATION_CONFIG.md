# ECHA.API Authentication Configuration

## JWT Configuration (appsettings.json)

Add the following to your `appsettings.json`:

```json
{
  "Jwt": {
	"Key": "your-secret-key-min-32-chars-long-for-security",
	"Issuer": "EconomiaComHistoria.API",
	"Audience": "EconomiaComHistoria.Client"
  }
}
```

⚠️ **IMPORTANT**: Change the `Key` to a strong, unique secret in production.

## CORS Configuration

Current setup (Development):
- **Policy**: "AllowAll"
- **Allows**: Any origin, method, and headers
- **Status**: ⚠️ Development only, NOT suitable for production

For **Production**, update the CORS policy in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
	options.AddPolicy("Production", policy =>
		policy
			.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials());
});
```

Then use: `app.UseCors("Production");`

## Swagger/OpenAPI Configuration

- **Enabled in Development**: Accessible at `/swagger/index.html`
- **Bearer Token Support**: Full integration with JWT authentication
- **Endpoints Documented**:
  - `POST /api/auth/register` - User registration
  - `POST /api/auth/login` - User login
  - `POST /api/auth/refresh` - Token refresh

### Testing in Swagger UI:

1. Call `POST /api/auth/register` or `POST /api/auth/login`
2. Copy the `accessToken` from the response
3. Click "Authorize" button (lock icon)
4. Paste: `Bearer <your_token_here>`
5. Make authenticated requests to protected endpoints

## Authentication Flow

### Register (New User)
```
POST /api/auth/register
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao@example.com",
  "password": "SecurePassword123!",
  "telemovel": "+244 912 345 678"
}

Response (201 Created):
{
  "userId": 1,
  "email": "joao@example.com",
  "nome": "João Silva",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2024-01-15T10:30:00Z"
}
```

### Login (Existing User)
```
POST /api/auth/login
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "SecurePassword123!"
}

Response (200 OK):
{
  "userId": 1,
  "email": "joao@example.com",
  "nome": "João Silva",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2024-01-15T10:30:00Z"
}
```

### Refresh Token (Get New Access Token)
```
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "a1b2c3d4e5f6..."
}

Response (200 OK):
{
  "userId": 1,
  "email": "joao@example.com",
  "nome": "João Silva",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "x1y2z3w4...",
  "expiresAt": "2024-01-15T10:30:00Z"
}
```

## Token Details

- **Access Token**: 
  - Expires: 4 hours
  - Used for API requests
  - Include in `Authorization: Bearer <token>` header

- **Refresh Token**:
  - Expires: 7 days (stored server-side in current implementation)
  - Used to obtain new access tokens
  - Should be stored securely (HttpOnly cookie recommended in production)

## Security Features

✅ **Implemented**:
- BCrypt password hashing (not plain text)
- JWT with HS256 signing algorithm
- Token expiration validation
- Role-based claims (Visitante, Registado, Premium, Professor, Admin)
- Input validation on authentication endpoints
- Unique email constraint in database

⚠️ **Recommendations for Production**:
- Store refresh tokens in database instead of memory
- Use HTTPS only
- Implement rate limiting on auth endpoints
- Add password complexity requirements
- Implement email verification
- Add refresh token revocation mechanism
- Use secure, HTTPOnly cookies for refresh tokens
- Implement device/session tracking

## Error Responses

All auth endpoints return appropriate HTTP status codes:

- `201 Created` - User successfully registered
- `200 OK` - Login successful, token refresh successful
- `400 Bad Request` - Missing or invalid input
- `401 Unauthorized` - Invalid credentials or token
- `409 Conflict` - Email already exists (registration)
