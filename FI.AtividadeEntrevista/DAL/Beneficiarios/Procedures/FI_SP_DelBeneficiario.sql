CREATE PROC FI_SP_DelBeneficiario
    @IdBeneficiario BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM BENEFICIARIOS
    WHERE ID = @IdBeneficiario;
END