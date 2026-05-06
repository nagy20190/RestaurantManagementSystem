# Project Analysis Report

## Existing Endpoints (What is need to refactor)

### AuthController `api/Auth`

- `POST /Login`
- `POST /Register`
- `GET /confirm-email`
- `POST /resend-confirmation`
- `POST /logout`


### SheduleOffersControllers `api/SheduleOffers`

- **Empty** — no endpoints at all

- `POST /schedule` — Schedule email offer job via HangFire

---

## Bugs & Issues Found

### Critical (may cause compile errors or crashes)

**UserController — `.Include(userRole)` compile error**

```csharp
var user = await _userRepository
    .FindByCondition(u => u.Id == id)
    .Include(userRole)   // ERROR: userRole is a string, Include() needs a lambda
    .FirstOrDefaultAsync();
```

Fix: Remove the `.Include(userRole)` line — `User` has no navigation property to include with a string.

---

**UserController — `DeleteUser` missing route param and `[Authorize]`**

```csharp
[HttpDelete]  // Missing /{id} route — id will never be bound from the request
public async Task<IActionResult> DeleteUser(int id)
```

Fix: Change to `[HttpDelete("{id}")]` and add `[Authorize(Roles = "SuperAdmin")]`.

---

**PaymentController — `.Result` on async method (deadlock risk)**

```csharp
CreatedBy = GetCurrentUserId().Result  // Blocking call inside async context
```

Fix: Use `await`: `CreatedBy = await GetCurrentUserId()`.

---

**ReservationController — `CreatedAtAction` used incorrectly**

```csharp
return CreatedAtAction("reservtion created", reservationDto);
// First arg must be a valid action name, not a message string
```

Fix: Use `return Ok(reservationDto)` or `return StatusCode(201, reservationDto)`.

---

### Program.cs Issues

- **Duplicate AutoMapper registration**: `AddAutoMapper(...)` AND a manual `AddSingleton<IMapper>` — one must be removed
- **Dead code after `app.Run()`**: The role-seeding block is unreachable — move it before `app.Run()`
- **Wrong type in role seed**: Uses `RoleManager<IdentityRole>` but Identity is registered with `RoleManager<Roles>` — will crash at runtime
- **`IHttpContextAccessor` not registered**: `JWTReader` uses it but `builder.Services.AddHttpContextAccessor()` is never called
- **`AddAuthorization()` called twice**

---

### Logic Bugs

- **UserController `UpdateUserRoles`** — roles are added inside the loop, then all roles are removed, then added again. The loop addition is wasted; remove it.
- **UserController `ChangePassword`** — password is changed via `UserManager` (correct) but then `PasswordHash` is manually set again (redundant double-hash).
- **OrderController `CreateOrder`** — `UserID` is taken from the request body, not from the JWT token. A user can create an order on behalf of another user's ID.
- **MealController `GetAvailableMeals`** — fetches all meals with no availability filtering. The `Meal` entity has no `IsAvailable` field.

---

### MappingProfiles Issues

- **`MealDTO` mapping will throw NullReferenceException** if `Restaurant` navigation property is not loaded (null guard needed)
- **`ReservationDTO.TableNumber` mapped from `src.Table.ID` (int)** — type mismatch, should be `.ToString()`
- **Missing mappings**: `CreateReservationDTO`, `Table`, `TableDTO`, `Inventory`, `UpdateUserProfileDTO`

---

### Naming & Convention Issues

- `SheduleOffersControllers` — typo ("Shedule" should be "Schedule") and plural "Controllers"
- `RestaurantController.GetAllRestaurants` uses route `/GetAll` — inconsistent with `MealController` which uses `GET /`
- `PaymentController` route `/Resturasnt/{resturasntId}/` has a typo ("Resturasnt")
- `UserController.UpdateUserRoles` valid roles hardcoded as `"User", "Admin", "SuperAdmin"` — missing `"RestaurantOwner"` which is used everywhere else

---

### Security Issues

- `UserController.UpdateUserRoles` has `[Authorize(Roles = "SuperAdmin")]` **commented out** — anyone can change any user's roles
- `ReservationController.CreateReservation` has no `[Authorize]`
- `UserController.DeleteUser` has no `[Authorize]`
