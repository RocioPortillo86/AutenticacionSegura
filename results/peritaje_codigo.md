# Peritaje de código

## Resumen ejecutivo
El análisis del código fuente del proyecto ASP.NET Web Forms revela varias áreas críticas que requieren atención para mejorar la seguridad, calidad y rendimiento del sistema. Se identificaron vulnerabilidades relacionadas con la gestión de sesiones, el uso de parámetros SQL, y la falta de validaciones adecuadas. Además, se observó un uso ineficiente de recursos en ciertas operaciones de base de datos. La implementación de prácticas recomendadas de OWASP y la optimización del acceso a datos son esenciales para mitigar riesgos y asegurar la integridad del sistema. Se recomienda priorizar las remediaciones de seguridad en los primeros 30 días, seguido de mejoras en rendimiento y calidad en los siguientes 60 a 90 días.

## Tabla de riesgos

| ID     | Categoría   | Severidad | Probabilidad | Archivo:Línea                  | Evidencia                                                                 | Impacto                                                                 | Remediación                                                                 | Parche mínimo                                                                 |
|--------|-------------|-----------|--------------|--------------------------------|---------------------------------------------------------------------------|------------------------------------------------------------------------|----------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| R-001  | Seguridad   | Alta      | Alta         | Codigo/Site.Master.cs:13       | Uso de sesiones sin validación de roles adecuada                          | Acceso no autorizado a funcionalidades administrativas                   | Implementar validación de roles antes de mostrar contenido sensible        | `if (Session["uid"] != null && Session["role"] != null && Session["role"].ToString() == "Admin") { pnlAdmin.Visible = true; } else { pnlAdmin.Visible = false; }` |
| R-002  | Seguridad   | Alta      | Media        | Codigo/Class/Services/SalesService.cs:14 | Cálculo de totales sin validación de stock disponible                     | Posibilidad de ventas con stock insuficiente                             | Validar stock antes de procesar la venta                                    | `if (ProductData.GetById(i.productId).Stock < i.qty) { throw new InvalidOperationException("Stock insuficiente"); }` |
| R-003  | Rendimiento | Media     | Media        | Codigo/Class/Data/ProductData.cs:53 | Uso de múltiples consultas para obtener productos                         | Incremento en el tiempo de respuesta debido a múltiples accesos a la DB  | Optimizar consultas para reducir el número de accesos a la base de datos    | `var products = ProductData.GetByIds(items.Select(i => i.productId));` |
| R-004  | Seguridad   | Alta      | Alta         | Codigo/Class/Services/AuthServices.cs:12 | Falta de protección contra ataques de fuerza bruta en el login            | Compromiso de cuentas de usuario                                        | Implementar bloqueo de cuenta tras múltiples intentos fallidos             | `if (failedAttempts >= 5) { throw new InvalidOperationException("Cuenta bloqueada temporalmente"); }` |

## Checklist

- [ ] OWASP Top 10
- [ ] Manejo de sesiones seguro
- [x] Uso de BCrypt para hashing de contraseñas
- [x] SQL parametrizado
- [ ] Protección contra XSS/CSRF
- [ ] Manejo adecuado de errores y logs
- [ ] Gestión segura de secretos y claves

## Prioridades 30/60/90 días

- **30 días**: Implementar validaciones de seguridad (roles, stock, protección contra fuerza bruta).
- **60 días**: Optimizar consultas a la base de datos para mejorar el rendimiento.
- **90 días**: Revisar y mejorar el manejo de errores y la gestión de secretos.

## Notas para ASP.NET Web Forms + ADO.NET

- Utilizar `SqlParameter` para prevenir inyecciones SQL.
- Implementar validación server-side para todas las entradas de usuario.
- Usar `Server.HtmlEncode` para prevenir XSS.
- Implementar control de roles para proteger recursos sensibles.
- Asegurar que las transacciones sean atómicas.
- Utilizar UTC para todas las operaciones de fecha y hora.
- Asegurar el redondeo correcto en cálculos monetarios.