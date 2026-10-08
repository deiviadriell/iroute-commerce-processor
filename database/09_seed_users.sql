USE IRouteCommerce;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.users WHERE username = 'admin')
    INSERT INTO dbo.users (username, password_hash, full_name)
    VALUES ('admin',
            'PBKDF2-SHA256$100000$gHPdnhmb00dzbUdF3DwNYA==$83RxXTvn+IMX4TBElBchc04IBwTZSrIOiReEDHIJ6ck=',
            N'Administrador');
GO
