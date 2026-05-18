# MemberNo Counter Table Design

## Table

```sql
CREATE TABLE public.member_no_counters (
    tenant_id uuid PRIMARY KEY,
    last_value integer NOT NULL
);
```

## Next Sequence SQL (atomic upsert)

```sql
INSERT INTO public.member_no_counters (tenant_id, last_value)
VALUES (@TenantId, 1)
ON CONFLICT (tenant_id)
DO UPDATE SET last_value = public.member_no_counters.last_value + 1
RETURNING last_value;
```

## MemberNo unique constraint (multi-tenant)

```sql
ALTER TABLE public.members
ADD CONSTRAINT ux_members_tenant_id_member_no UNIQUE (tenant_id, member_no);
```
