-- Inclusão do campo CPF na tabela CLIENTES
-- CPF gravado apenas com dígitos (11 caracteres), obrigatório e único

ALTER TABLE CLIENTES ADD CPF VARCHAR (11) NOT NULL;
GO

ALTER TABLE CLIENTES ADD CONSTRAINT UQ_CLIENTES_CPF UNIQUE (CPF);
GO
