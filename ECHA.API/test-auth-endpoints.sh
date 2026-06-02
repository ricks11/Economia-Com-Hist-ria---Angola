#!/bin/bash
# ECHA.API Authentication Endpoints Testing Script

API_URL="https://localhost:5001"  # Change to your API URL

echo "=========================================="
echo "ECHA.API Authentication Testing"
echo "=========================================="

# Test 1: Register a new user
echo -e "\n[TEST 1] POST /api/auth/register"
echo "Registering new user..."
REGISTER_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
	"nome": "João Silva",
	"email": "joao.silva@example.com",
	"password": "SecurePassword123!",
	"telemovel": "+244 912 345 678"
  }')

echo "Response:"
echo "$REGISTER_RESPONSE" | jq '.'

# Extract tokens
ACCESS_TOKEN=$(echo "$REGISTER_RESPONSE" | jq -r '.accessToken')
REFRESH_TOKEN=$(echo "$REGISTER_RESPONSE" | jq -r '.refreshToken')

echo -e "\nAccess Token: ${ACCESS_TOKEN:0:50}..."
echo "Refresh Token: ${REFRESH_TOKEN:0:50}..."

# Test 2: Try to register the same email again (should fail)
echo -e "\n[TEST 2] POST /api/auth/register (Duplicate email - Should fail)"
curl -s -X POST "$API_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
	"nome": "Another User",
	"email": "joao.silva@example.com",
	"password": "DifferentPassword123!"
  }' | jq '.'

# Test 3: Login with correct credentials
echo -e "\n[TEST 3] POST /api/auth/login (Valid credentials)"
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "joao.silva@example.com",
	"password": "SecurePassword123!"
  }')

echo "Response:"
echo "$LOGIN_RESPONSE" | jq '.'

NEW_ACCESS_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.accessToken')
NEW_REFRESH_TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.refreshToken')

# Test 4: Login with incorrect password
echo -e "\n[TEST 4] POST /api/auth/login (Invalid password - Should fail)"
curl -s -X POST "$API_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "joao.silva@example.com",
	"password": "WrongPassword123!"
  }' | jq '.'

# Test 5: Refresh token
echo -e "\n[TEST 5] POST /api/auth/refresh (Refresh access token)"
REFRESH_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d "{
	\"refreshToken\": \"$REFRESH_TOKEN\"
  }")

echo "Response:"
echo "$REFRESH_RESPONSE" | jq '.'

# Test 6: Test invalid refresh token
echo -e "\n[TEST 6] POST /api/auth/refresh (Invalid refresh token - Should fail)"
curl -s -X POST "$API_URL/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
	"refreshToken": "invalid-token-12345"
  }' | jq '.'

echo -e "\n=========================================="
echo "Testing Complete!"
echo "=========================================="
