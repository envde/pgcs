CREATE VIEW active_users_with_orders AS
SELECT
    u.id,
    u.username,
    u.email,
    u.full_name,
    u.status,
    u.is_premium,
    u.balance,
    u.loyalty_points,
    COUNT(DISTINCT o.id) AS total_orders,
    COALESCE(SUM(o.total), 0) AS total_spent,
    COALESCE(AVG(o.total), 0) AS average_order_value,
    MAX(o.created_at) AS last_order_date,
    ARRAY_AGG(DISTINCT o.status ORDER BY o.status) FILTER (WHERE o.status IS NOT NULL) AS order_statuses,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'pending') AS pending_orders,
    COUNT(DISTINCT o.id) FILTER (WHERE o.status = 'delivered') AS completed_orders
FROM users u
         LEFT JOIN orders o ON u.id = o.user_id AND o.cancelled_at IS NULL
WHERE u.status = 'active' AND u.is_deleted = FALSE
GROUP BY u.id, u.username, u.email, u.full_name, u.status, u.is_premium, u.balance, u.loyalty_points;