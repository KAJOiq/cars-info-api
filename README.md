
# Application Payment API

## Postman Test Collection :
- I have uploaded the Postman collection here:
The name of Collection `APP_API_PAY.postman_collection.json`.
[Download the Postman Collection]()
- After download it, open it with Postman.
- Test all endpoint below.

## Authentication and Authorization :

### 1. Username and Password to Generate Token
   - Send the username and password to the authentication endpoint to receive a bearer token.
   - The token is used to authenticate subsequent requests.


### 2. Bearer Token Sent with Requests:
   - Include the bearer token in the Authorization header for subsequent requests to access the application data.

## Endpoints :

### 1. Login and Get Bearer Token
   - **Endpoint:** `POST http://localhost:8050/api/auth/login`
   - **Note:** we will give you `username` and `password` privately.
   - **Request:**
     ```json
     {
       "username": "*******",
       "password": "*******"
     }
     ```
   - **Response:**
     - **Success (200):**
       ```json
       {
         "isSuccess": true,
         "results": {
           "Token": "***********************************************************
             ************************************************"
         },
         "errors": []
       }
       ```
     - **Error (401):**
       ```json
        {
            "isSuccess": false,
            "results": null,
            "errors": [
                {
                    "code": "401",
                    "message": "Invalid credentials"
                }
            ]
        }
       ```
   

### 2. Get Application Data 
   - **Endpoint:** `GET http://localhost:8050/api/user/:appid`
   - **Request Parameters:**
     - `appid`: Application ID (send in long format).
   - **Headers:**
    

| Header          | Description                                        |
|:--------------- |:-------------------------------------------------- |
| `UserName`      | Your username                                      |
| `Password`      | Your password                                      |
| `Token`         | Bearer token generated from login (SHA256 hashed). |
| `Accept`        | `application/json, text/plain, */*`                |
| `Authorization` | `Bearer <Token>` from login.                       |
| `Content-Type`  | `application/json`.                                |


   - **Response:**
     - **Success (200):**
       ```json
         {
            "isSuccess": true,
            "results": {
                "givenName": "الاسم الاول",
                "fatherName": "اسم الاب",
                "grandfatherName": "اسم الجد",
                "motherName": "اسم الام",
                "motherFatherName": "اسم اب الام",
                "UseCase": "نوع المعاملة",
                "licenseNumber": "رقم السيارة بالعربي",
                "licenseNumberLatin": "رقم السيارة بالانكليزي",
                "governorate": "المحافظة",
                "usage": "خصوصي أو اجرة .. الخ",
                "passengers": "عدد المقاعد",
                "vehicleCategory": "فئة المركبة",
                "cylinders": "عدد الاسطوانات",
                "axis": "اذا كانت دفع رباعي .. الخ",
                "cabinType": "اعتيادي او مصفح .. الخ",
                "loadWeight": "وزن الحمولة للحمل",
                "dateOfIssue": "تاريخ الإصدار",
                "dateOfExpiry": "تاريخ النفاذ",
                "dlCategory": "الفئة الخاصة بالإجازات فقط",
                "idCurrentState": "حالة المعاملة (لأي مرحلة وصلت)"
            },
            "errors": []
        }
       ```
     - **Error (404):**
       ```json
        {
            "isSuccess": false,
            "results": null,
            "errors": [
                {
                    "code": "404",
                    "message": "Application not found."
                }
            ]
        }
       ```
---
## Notes :

- You must include the Bearer token in the `Authorization` header when requesting application data.
- The Bearer token will expire 24 hours after creation.
- Some fields may be null based on Application Type.