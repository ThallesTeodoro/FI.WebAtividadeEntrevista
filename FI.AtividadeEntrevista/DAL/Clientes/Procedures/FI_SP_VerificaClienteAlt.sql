﻿CREATE PROC FI_SP_VerificaClienteAlt
	@CPF VARCHAR(14),
	@ID bigint
AS
BEGIN
	SELECT 1 FROM CLIENTES WHERE CPF = @CPF AND ID <> @ID
END