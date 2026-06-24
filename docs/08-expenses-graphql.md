# Step 8 — Expenses (GraphQL CRUD + FluentValidation)

The main data feature. Full GraphQL CRUD, user-scoped, with FluentValidation and a
RESTRICT FK so categories in use can't be deleted.

> Status: ✅ complete. Full CRUD + FluentValidation + two-level ownership + RESTRICT,
> all verified over GraphQL (12 scenarios).

---

## Entity

```
Expense: Id, UserId, CategoryId, Amount (decimal), Note, SpentAt, CreatedAt
```

- **Amount is `decimal`** (never `double` — float rounding corrupts money).
  Postgres `numeric`; GraphQL `Decimal`.
- FKs to **User** (cascade) and **Category** (**RESTRICT** — see below).

## New concepts

### FluentValidation
Validation rules move from inline `if`s into dedicated `AbstractValidator<T>`
classes (declarative, reusable, unit-testable). The **service** injects the
validator, calls `ValidateAsync`, and maps failures to `Result.Validation`.
Example rules: `Amount > 0`, `Note` max length, `SpentAt <= now`.

### RESTRICT foreign key
`Expense → Category` uses `DeleteBehavior.Restrict`: deleting a category that has
expenses is rejected by Postgres; we surface it as a `Conflict`. (Decided in
Step 7.) `Expense → User` stays cascade.

## Two-level ownership (security)

Create/update must verify **both**:
1. The expense is the user's (scoped load on update/delete).
2. The passed `CategoryId` **belongs to the user** — you can't attach an expense to
   someone else's category. Easy to miss; checked explicitly.

## Query shape

`myExpenses(categoryId, from, to)` — optional filters (category + date range),
built up as LINQ `Where` clauses. Plain list for now; paging in Step 14.

## Decisions

- `myExpenses` with optional filter args.
- Validators run **explicitly in the service** (not via HC middleware).

## Chunks

- [x] 8a — `Expense` entity + config (numeric(18,2), FKs; RESTRICT on Category) + `DbSet`
- [x] 8b — `AddExpenses` migration; applied; FK rules verified (Category RESTRICT, User CASCADE)
- [x] 8c — FluentValidation 12; `ExpenseDto` + inputs + `Create/UpdateExpenseInputValidator`
- [x] 8d — `ExpenseService` (scoped CRUD, OwnsCategoryAsync, validators, filters)
- [x] 8e — `ExpenseQueries`/`ExpenseMutations` type extensions + register
- [x] 8f — built + tested (12 scenarios)

## RESTRICT delete — fixed a leaky error

Deleting an in-use category first surfaced as "Unexpected Execution Error" — the
Postgres FK violation threw `DbUpdateException`, unhandled. Fixed in
`CategoryService.DeleteAsync` with a pre-check (`db.Expenses.AnyAsync(...)`) →
returns a clean `Conflict` ("Category has expenses and cannot be deleted.") before
hitting the constraint. (Global GraphQL exception handling comes in Step 9.)

## Verified behavior

| Scenario | Result |
|----------|--------|
| createExpense (valid) | returns expense |
| amount = 0 | `VALIDATION` |
| future SpentAt | `VALIDATION` |
| category not owned | `NOTFOUND` (two-level ownership) |
| myExpenses (owner) | lists own |
| myExpenses (other user) | `[]` isolated |
| myExpenses date filter | filters in SQL |
| update other user's expense | `NOTFOUND` |
| update own | works |
| delete in-use category | `CONFLICT` (RESTRICT) |
| delete expense, then category | both succeed |

Request collection: `http/expenses.http`.
