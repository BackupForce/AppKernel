## 角色管理 API

> Base Path: `/api/v1/roles`

所有端點皆需攜帶 JWT，並通過對應的權限：
- 檢視：`roles:view`
- 建立：`roles:create`
- 更新/調整權限：`roles:update`
- 刪除：`roles:delete`

### 建立角色
- **POST** `/`
- **Body**
  ```json
  {
    "name": "Admin"
  }
  ```
- **Response**
  - `200 OK`: `int`（新角色的 `id`）
  - `400 Bad Request`: 驗證錯誤（名稱空白或重複）

### 更新角色名稱
- **PUT** `/{id}`
- **Body**
  ```json
  {
    "name": "Administrator"
  }
  ```
- **Response**
  - `200 OK`
  - `400 Bad Request`: 驗證錯誤或名稱衝突
  - `404 Not Found`: 角色不存在

### 刪除角色
- **DELETE** `/{id}`
- **Response**
  - `200 OK`
  - `404 Not Found`: 角色不存在  
  > 刪除時會連同該角色底下的權限紀錄一併刪除。

### 取得角色詳情
- **GET** `/{id}`
- **Response**
  ```json
  {
    "id": 1,
    "name": "Admin",
    "permissionCodes": [
      "roles:view",
      "roles:update"
    ]
  }
  ```
  - `404 Not Found`: 角色不存在

### 角色列表
- **GET** `/`
- **Response**
  ```json
  [
    {
      "id": 1,
      "name": "Admin",
      "permissionCount": 5
    }
  ]
  ```

### 取得角色權限
- **GET** `/{id}/permissions`
- **Response**
  ```json
  [
    "roles:view",
    "roles:create"
  ]
  ```
  - `404 Not Found`: 角色不存在

### 新增角色權限（批次）
- **POST** `/{id}/permissions`
- **Body**
  ```json
  {
    "permissionCodes": [
      "roles:view",
      "roles:create"
    ]
  }
  ```
- **Response**
  - `200 OK`（idempotent，已存在的 code 會被略過）
  - `400 Bad Request`: 權限代碼為空
  - `404 Not Found`: 角色不存在

### 移除角色權限（批次）
- **POST** `/{id}/permissions/remove`
- **Body**
  ```json
  {
    "permissionCodes": [
      "roles:create",
      "roles:update"
    ]
  }
  ```
- **Response**
  - `200 OK`（不存在的 code 會被忽略）
  - `400 Bad Request`: 權限代碼為空
  - `404 Not Found`: 角色不存在
