CREATE PROC FI_SP_ConsBeneficiario
	@IDCLIENTE BIGINT
AS
BEGIN
	SELECT NOME, CPF, IDCLIENTE, ID FROM BENEFICIARIOS WHERE IDCLIENTE = @IDCLIENTE
END