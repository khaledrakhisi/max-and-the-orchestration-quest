"""
Test script for authentication endpoints
"""
import requests
import json

BASE_URL = "http://localhost:8000"

def test_register():
    """Test user registration"""
    print("=" * 50)
    print("Testing REGISTER endpoint...")
    print("=" * 50)
    
    data = {
        "username": "testplayer",
        "email": "testplayer@example.com",
        "password": "securePass123"
    }
    
    response = requests.post(f"{BASE_URL}/register", json=data)
    
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    if response.status_code == 201:
        print("✅ Registration successful!")
        return response.json()
    else:
        print("❌ Registration failed!")
        return None

def test_login(email, password):
    """Test user login"""
    print("\n" + "=" * 50)
    print("Testing LOGIN endpoint...")
    print("=" * 50)
    
    data = {
        "email": email,
        "password": password
    }
    
    response = requests.post(f"{BASE_URL}/login", json=data)
    
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    if response.status_code == 200:
        print("✅ Login successful!")
        return response.json()
    else:
        print("❌ Login failed!")
        return None

def test_login_wrong_password(email):
    """Test login with wrong password"""
    print("\n" + "=" * 50)
    print("Testing LOGIN with WRONG password...")
    print("=" * 50)
    
    data = {
        "email": email,
        "password": "wrongpassword"
    }
    
    response = requests.post(f"{BASE_URL}/login", json=data)
    
    print(f"Status Code: {response.status_code}")
    print(f"Response: {json.dumps(response.json(), indent=2)}")
    
    if response.status_code == 401:
        print("✅ Correctly rejected wrong password!")
    else:
        print("❌ Should have rejected wrong password!")

if __name__ == "__main__":
    print("\n🧪 Starting Authentication Tests...\n")
    
    # Test registration
    register_result = test_register()
    
    if register_result:
        email = "testplayer@example.com"
        password = "securePass123"
        
        # Test successful login
        login_result = test_login(email, password)
        
        # Test failed login
        test_login_wrong_password(email)
        
        if login_result:
            print("\n" + "=" * 50)
            print("Access Token:")
            print("=" * 50)
            print(login_result["access_token"])
            print("\nUse this token in Authorization header:")
            print(f"Authorization: Bearer {login_result['access_token']}")
    
    print("\n✨ Tests completed!")
