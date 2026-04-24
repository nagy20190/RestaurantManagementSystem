---
name: Project Analysis Report
overview: Full analysis of all controllers — existing endpoints, missing endpoints, and bugs/issues found across the project.
todos:
  - id: fix-critical-bugs
    content: "Fix critical bugs: UserController .Include(userRole) compile error, DeleteUser missing route/auth, PaymentController .Result deadlock, ReservationController CreatedAtAction misuse"
    status: in_progress
  - id: fix-program-cs
    content: "Fix Program.cs: remove duplicate AutoMapper, move role seed before app.Run(), fix IdentityRole vs Roles type, add AddHttpContextAccessor(), remove duplicate AddAuthorization()"
    status: pending
  - id: fix-logic-bugs
    content: "Fix logic bugs: UpdateUserRoles double-add, ChangePassword double-hash, OrderController UserID from JWT not body, MealController GetAvailableMeals filter"
    status: pending
  - id: fix-mappings
    content: "Fix MappingProfiles: duplicate Reservation map, null guard on Meal.Resturant, TableNumber type mismatch, add missing mappings for CreateReservationDTO/Table/Inventory/UpdateUserProfileDTO"
    status: pending
  - id: implement-reservation
    content: "Implement missing ReservationController endpoints: GET /{id}, GET /user/{userId}, PUT /update/{id}, DELETE /delete/{id}, GET /availability"
    status: pending
  - id: create-table-controller
    content: Create TableController with full CRUD + GET /restaurant/{restaurantId}
    status: pending
  - id: create-inventory-controller
    content: Create InventoryController with full CRUD + GET /meal/{mealId}
    status: pending
  - id: implement-schedule-offers
    content: Implement SheduleOffersControllers (rename to ScheduleOffersController) with HangFire-based email scheduling endpoint
    status: pending
  - id: todo-1775471847168-vaum9k71g
    content: ""
    status: pending
isProject: false
---

# Project Analysis Report

## Existing Endpoints (What is Already Implemented)

### AuthController `api/Auth`

- `POST /Login`
- `POST /Register`
- `GET /confirm-email`
- `POST /resend-confirmation`
- `POST /logout`

### CategoryController `api/Category`

- `POST /` — Create (SuperAdmin only)
- `GET /GetAll` — Paginated list with search
- `GET /{id}` — Get by ID
- `PUT /{id}` — Update (SuperAdmin only)
- `DELETE /{id}` — Hard delete (SuperAdmin only)

### MealController `api/Meal`

- `GET /` — Paged list
- `GET /{id}` — Get by ID
- `POST /` — Create (SuperAdmin, RestaurantOwner)
- `PUT /{id}` — Update (SuperAdmin, RestaurantOwner)
- `DELETE /{id}` — Delete (SuperAdmin, RestaurantOwner)
- `GET /search` — Filter by name, price, restaurant, category
- `GET /restaurant/{restaurantId}` — Meals by restaurant
- `GET /category/{categoryId}` — Meals by category
- `GET /popular` — Top N popular meals
- `GET /available` — (bug: returns all meals, not filtered)

### OrderController `api/Order`

- `GET /` — Paged list
- `GET /{id}` — Order with items
- `POST /` — Create order (Authorized)
- `PUT /{id}/status` — Update status (SuperAdmin, RestaurantOwner)
- `DELETE /{id}` — Cancel order (Authorized)
- `GET /user/{userId}` — Order history for user
- `GET /restaurant/{restaurantId}` — Orders for restaurant
- `GET /status/{status}` — Filter by status
- `GET /tracking/{id}` — Order tracking
- `GET /statistics` — Order stats
- `GET /search` — Search by filters

### PaymentController `api/Payment`

- `POST /process` — Process payment (Authorized)
- `POST /confirm/{paymentId}` — Confirm payment (SuperAdmin)
- `GET /{id}` — Get by ID (SuperAdmin)
- `GET /order/{orderId}` — Get by order (SuperAdmin)
- `GET /by-status/{status}` — Filter by status (SuperAdmin)
- `GET /search` — Search payments (SuperAdmin)
- `GET /user/{userId}/payments` — User's payments (SuperAdmin)
- `GET /Resturasnt/{resturasntId}/` — Restaurant payments (SuperAdmin)
- `PUT /{paymentId}/status` — Update status (SuperAdmin)
- `GET /available-methods` — List payment methods
- `GET /reports/monthly` — Monthly reports (SuperAdmin)
- `GET /analytics` — Analytics (SuperAdmin)

### RestaurantController `api/Restaurant`

- `GET /search` — Search with filters and pagination
- `GET /GetAll` — Paged list
- `GET /{id}` — Get with meals and categories
- `GET /{restaurantid}/menu` — Restaurant menu
- `GET /{restaurantid}/reviews` — Restaurant reviews
- `POST /` — Create (SuperAdmin, RestaurantOwner)
- `PUT /{id}` — Update (SuperAdmin, RestaurantOwner)
- `DELETE /{id}` — Delete (SuperAdmin, RestaurantOwner)
- `GET /{id}/categories` — Menu categories
- `POST /{id}/reviews` — Add review (Authorized)
- `GET /{id}/reservations` — Restaurant reservations

### ReservationController `api/Reservation`

- `POST /create` — Create reservation (**only this one works**)
- All other endpoints are **commented out**

### UserController `api/User`

- `GET /current` — Get own profile (Authorized)
- `PUT /current` — Update own profile (Authorized)
- `POST /current/change-password` — Change password (Authorized)
- `POST /forgot-password`
- `POST /reset-password`
- `DELETE /current` — Soft-delete own account (Authorized)
- `GET /GetAll` — All users paginated (SuperAdmin)
- `GET /active` — Active users (SuperAdmin)
- `GET /{id}` — Get user by ID (SuperAdmin, RestaurantOwner)
- `DELETE /` — Soft-delete user **(broken — no route param)**
- `PATCH /{id}/restore` — Restore user (SuperAdmin)
- `PUT /{id}/roles` — Update roles **(Authorize commented out)**

### SheduleOffersControllers `api/SheduleOffers`

- **Empty** — no endpoints at all

---

## Missing Endpoints to Implement

### ReservationController (mostly commented out)

- `GET /{id}` — Get reservation by ID
- `GET /user/{userId}` — Get reservations by user
- `PUT /update/{id}` — Update reservation
- `DELETE /delete/{id}` — Cancel/delete reservation
- `GET /availability` — Check table availability by restaurant + date

### TableController (does not exist — entity + DbSet both exist)

- `GET /` — Get all tables
- `GET /{id}` — Get table by ID
- `POST /` — Create table (SuperAdmin, RestaurantOwner)
- `PUT /{id}` — Update table (SuperAdmin, RestaurantOwner)
- `DELETE /{id}` — Delete table (SuperAdmin, RestaurantOwner)
- `GET /restaurant/{restaurantId}` — Tables by restaurant

### InventoryController (does not exist — entity + DbSet both exist)

- `GET /` — Get all inventory
- `GET /{id}` — Get by ID
- `POST /` — Create entry (SuperAdmin, RestaurantOwner)
- `PUT /{id}` — Update quantity (SuperAdmin, RestaurantOwner)
- `DELETE /{id}` — Delete entry (SuperAdmin)
- `GET /meal/{mealId}` — Inventory for a meal

### ReviewController (no dedicated controller)

- Reviews are only addable via `RestaurantController`
- Missing: `GET /{id}`, `PUT /{id}`, `DELETE /{id}`

### SheduleOffersControllers (empty)

- `POST /schedule` — Schedule email offer job via HangFire

---

## Bugs & Issues Found

### Critical (may cause compile errors or crashes)

**UserController — `.Include(userRole)` compile error**

```7:413:DeliveryManagementSystem/Controllers/UserController.cs
var user = await _userRepository
    .FindByCondition(u => u.Id == id)
    .Include(userRole)   // ERROR: userRole is a string, Include() needs a lambda
    .FirstOrDefaultAsync();
```

Fix: Remove the `.Include(userRole)` line — `User` has no navigation property to include with a string.

**UserController — `DeleteUser` missing route param and `[Authorize]`**

```7:448:DeliveryManagementSystem/Controllers/UserController.cs
[HttpDelete]  // Missing /{id} route — id will never be bound from the request
public async Task<IActionResult> DeleteUser(int id)
```

Fix: Change to `[HttpDelete("{id}")]` and add `[Authorize(Roles = "SuperAdmin")]`.

**PaymentController — `.Result` on async method (deadlock risk)**

```7:85:DeliveryManagementSystem/Controllers/PaymentController.cs
CreatedBy = GetCurrentUserId().Result  // Blocking call inside async context
```

Fix: Make the surrounding `ProcessPayment` method properly `await` this: `CreatedBy = await GetCurrentUserId()`.

**ReservationController — `CreatedAtAction` used incorrectly**

```7:24:DeliveryManagementSystem/Controllers/ReservationController.cs
return CreatedAtAction("reservtion created", reservationDto);
// First arg must be a valid action name, not a message string
```

Fix: Use `return Ok(reservationDto)` or `return StatusCode(201, reservationDto)`.

### Program.cs Issues

- **Duplicate AutoMapper registration**: `AddAutoMapper(...)` at line ~29 AND a manual `AddSingleton<IMapper>` later — one must be removed (keep the manual one since `MappingProfiles` is in `BLL.Healpers` namespace)
- **Dead code after `app.Run()`**: The role-seeding block (lines ~215–234) is unreachable — move it before `app.Run()`
- **Wrong type in role seed**: Uses `RoleManager<IdentityRole>` but Identity is registered with `RoleManager<Roles>` — will crash at runtime
- `**IHttpContextAccessor` not registered**: `JWTReader` uses it but `builder.Services.AddHttpContextAccessor()` is never called
- `**AddAuthorization()` called twice** (lines ~22 and ~165)

### Logic Bugs

- **UserController `UpdateUserRoles`** — roles are added, then all removed, then added again. The first `AddToRoleAsync` inside the loop is wasted work; remove the loop addition.
- **UserController `ChangePassword`** — password is changed via `UserManager` (correct) but then `PasswordHash` is manually set again (redundant double-hash).
- **OrderController `CreateOrder`** — `UserID` is taken from the request body, not from the JWT token. A user can create an order for another user's ID.
- **MealController `GetAvailableMeals`** — fetches all meals; no availability filtering (the `Meal` entity has no `IsAvailable` field).

### MappingProfiles Issues

- `**Reservation → ReservationDTO` mapped twice** (line 58 and 59) — second mapping overrides the first
- `**MealDTO` mapping will NullReferenceException** if `Resturant` navigation property is not loaded
- `**ReservationDTO.TableNumber` mapped from `src.Table.ID` (int)** — type mismatch, should map `Table.ID.ToString()` or use a proper `TableNumber` field
- **Missing mappings**: `CreateReservationDTO`, `Table`, `Inventory`, `UpdateUserProfileDTO`, `TableDTO`

### Naming & Convention Issues

- `SheduleOffersControllers` — typo ("Shedule" should be "Schedule") and plural "Controllers"
- `RestaurantController.GetAllRestaurants` uses route `GetAll` instead of the default `GET /`; inconsistent with `MealController` which uses `GET /`
- `PaymentController` route `/Resturasnt/{resturasntId}/` has a typo ("Resturasnt")
- `UserController.UpdateUserRoles` valid roles hardcoded as `"User", "Admin", "SuperAdmin"` — missing `"RestaurantOwner"` which is used everywhere else

### Security Issues

- `UserController.UpdateUserRoles` has `[Authorize(Roles = "SuperAdmin")]` **commented out** — anyone can change any user's roles
- `ReservationController.CreateReservation` has no `[Authorize]`
- `UserController.DeleteUser` has no `[Authorize]`

