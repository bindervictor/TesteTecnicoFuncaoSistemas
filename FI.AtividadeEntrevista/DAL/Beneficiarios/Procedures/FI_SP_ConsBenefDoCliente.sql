CREATE PROC FI_SP_ConsBenefDoCliente
    @IdCliente BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ID        AS Id,
        CPF       AS CPF,
        NOME      AS Nome,
        IDCLIENTE AS IdCliente
    FROM dbo.BENEFICIARIOS WITH (NOLOCK)
    WHERE IDCLIENTE = @IdCliente
    ORDER BY NOME;
END