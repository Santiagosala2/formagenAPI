# Formagen API Reference

### Auth

Uses session cookie for auth, with Azure Cosmos DB available as a session state provider.

An OTP is sent to all roles to grant access to the app's routes. An HTTP Azure Logic Apps is used (as a service) by Formagen API to send the OTP to the users

#### Roles

| Role  |             Access             |  Cookie name  |
| :---- | :----------------------------: | :-----------: |
| Admin | /dashboard/\*, /build, /submit |   SessionId   |
| User  |        /access, /submit        | UserSessionId |

## Endpoints

|                | Admin                  |              |
| -------------- | ---------------------- | ------------ |
| Operation Name | Endpoint               | Request Type |
| sendOTP        | `/admin/otp`           | POST         |
| verifyOTP      | `/admin/verifyOtp`     | POST         |
| getSession     | `/admin/user`          | GET          |
| createUser     | `/admin/user`          | POST         |
| getAllUsers    | `/admin/users`         | GET          |
| deleteUser     | `/admin/user/{userId}` | DELETE       |
| saveUser       | `/admin/updateUser`    | POST         |

|                  | Form                       |              |
| ---------------- | -------------------------- | ------------ |
| Operation Name   | Endpoint                   | Request Type |
| createForm       | `/form`                    | POST         |
| getForm          | `/form/{formId}`           | GET          |
| deleteForm       | `/form/{formId}`           | DELETE       |
| saveForm         | `/form/{form.id}`          | POST         |
| getAllForms      | `/form`                    | GET          |
| shareForm        | `/form/share`              | POST         |
| submitForm       | `/form/submit`             | POST         |
| removeAccessForm | `/form/removeAccess`       | POST         |
| getFormResponses | `/form/{formId}/responses` | GET          |

|                | User              |              |
| -------------- | ----------------- | ------------ |
| Operation Name | Endpoint          | Request Type |
| sendOTP        | `/user/otp`       | POST         |
| verifyOTP      | `/user/verifyOtp` | POST         |
| getSession     | `/user`           | GET          |
| createUser     | `/user`           | POST         |
| getAllUsers    | `/users`          | GET          |
| deleteUser     | `/user/{userId}`  | DELETE       |
| saveUser       | `/updateUser`     | POST         |
