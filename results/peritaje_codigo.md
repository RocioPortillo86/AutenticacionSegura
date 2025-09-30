# Peritaje de código

## Resumen ejecutivo
El análisis del código fuente del proyecto ASP.NET Web Forms con ADO.NET y SQL Server revela varias áreas críticas que requieren atención inmediata para mejorar la seguridad, rendimiento, calidad y mantenibilidad del sistema. Se han identificado problemas de seguridad relacionados con la gestión de sesiones, el uso de parámetros SQL y la validación de entradas. Además, se observan oportunidades para optimizar el rendimiento mediante la reducción de llamadas repetitivas a la base de datos y mejorar la calidad del código al reducir la duplicación y la complejidad. La implementación de prácticas recomendadas de OWASP y la mejora en el manejo de errores y registros son esenciales para mitigar riesgos potenciales. Este informe detalla los riesgos identificados, su impacto y las recomendaciones para su remediación, priorizando acciones a corto, mediano y largo plazo.

## Tabla de riesgos

| ID    | Categoría   | Severidad | Probabilidad | Archivo:Línea                  | Evidencia                                                                 | Impacto                                                                 | Remediación                                                                 | Parche mínimo                                                                 |
|-------|-------------|-----------|--------------|--------------------------------|---------------------------------------------------------------------------|-------------------------------------------------------------------------|----------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| R-001 | Seguridad   | Alta      | Alta         | Codigo/Site.Master.cs:10       | Uso de `Session` sin validación de rol adecuada.                          | Acceso no autorizado a funcionalidades administrativas.                  | Implementar validación de roles en el servidor.                             | `if (Session["uid"] != null && Session["role"] != null && Session["role"].ToString() == "Admin") { pnlAdmin.Visible = true; } else { pnlAdmin.Visible = false; }` |
| R-002 | Rendimiento | Media     | Media        | Codigo/Class/Services/SalesService.cs:11 | Llamadas repetitivas a `ProductData.GetById` dentro de un bucle.          | Degradación del rendimiento debido a múltiples accesos a la base de datos. | Almacenar resultados en una variable temporal para evitar llamadas repetidas. | `var products = items.Select(i => ProductData.GetById(i.productId)).ToDictionary(p => p.Id);` |
| R-003 | Seguridad   | Alta      | Alta         | Codigo/Class/Services/AuthServices.cs:12 | Falta de validación de entrada para el campo `email`.                     | Posibilidad de inyección SQL o XSS.                                      | Validar y sanitizar entradas de usuario antes de procesarlas.               | `if (!IsValidEmail(email)) { throw new ArgumentException("Email inválido."); }` |
| R-004 | Seguridad   | Alta      | Alta         | Codigo/Class/Data/UserData.cs:54 | Uso de `AddWithValue` sin especificar tipo de dato para `@Email`.         | Riesgo de inyección SQL.                                                 | Usar `SqlParameter` con tipo de dato explícito.                             | `cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 255) { Value = email.Trim() });` |

## Checklist

- [ ] OWASP Top 10
- [ ] Gestión de sesiones segura
- [x] Uso de BCrypt para hashing de contraseñas
- [x] SQL parametrizado
- [ ] Protección contra XSS/CSRF
- [ ] Manejo adecuado de errores y logs
- [ ] Gestión segura de secretos/keys

## Prioridades 30/60/90 días

- **30 días**: Implementar validación de roles y sanitización de entradas. Revisar y corregir el uso de `AddWithValue`.
- **60 días**: Optimizar el rendimiento reduciendo llamadas repetitivas a la base de datos. Implementar manejo de errores y logs.
- **90 días**: Revisar y asegurar la protección contra XSS/CSRF. Mejorar la gestión de secretos y claves.

## Notas para ASP.NET Web Forms + ADO.NET

- Utilizar `SqlParameter` para todas las consultas SQL para prevenir inyecciones.
- Implementar validación server-side para todas las entradas de usuario.
- Usar `Server.HtmlEncode` para prevenir XSS.
- Implementar control de roles en el servidor para asegurar el acceso adecuado.
- Utilizar transacciones atómicas para operaciones de base de datos críticas.
- Usar UTC para todas las operaciones de fecha y hora.
- Asegurar el redondeo correcto en operaciones monetarias para evitar errores de cálculo.