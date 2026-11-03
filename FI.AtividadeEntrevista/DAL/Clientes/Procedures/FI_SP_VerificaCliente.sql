CREATE PROC FI_SP_VerificaCliente
	@CPF VARCHAR(11),
	@ID BIGINT = 0
AS
BEGIN
	-- Retorna linha se o CPF já pertence a outro cliente (@ID = 0 na inclusão; Id do próprio cliente na alteração)
	SELECT 1 FROM CLIENTES WHERE CPF = @CPF AND (@ID = 0 OR ID <> @ID);
END