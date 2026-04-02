using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tab.Migrations
{
    /// <inheritdoc />
    public partial class CriarPost1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Agencias_AgenciaId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Cadastros_CadastroId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Contas_ContaId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_TipoEntradas_TipoEntradaId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Usuarios_UsuarioId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Agencias_AgenciaId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Cadastros_CadastroId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Contas_ContaId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_TipoDespesas_TipoDespesaId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Usuarios_UsuarioId",
                table: "Saidas");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Saidas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TipoDespesaId",
                table: "Saidas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DataReferencia",
                table: "Saidas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContaId",
                table: "Saidas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CadastroId",
                table: "Saidas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AgenciaId",
                table: "Saidas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataCadastro",
                table: "Recebimentos",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataActualizacao",
                table: "Recebimentos",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataCadastro",
                table: "ParticipanteEventos",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Entradas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TipoEntradaId",
                table: "Entradas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContaId",
                table: "Entradas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CadastroId",
                table: "Entradas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AgenciaId",
                table: "Entradas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataCadastro",
                table: "EntradaEventos",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Agencias_AgenciaId",
                table: "Entradas",
                column: "AgenciaId",
                principalTable: "Agencias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Cadastros_CadastroId",
                table: "Entradas",
                column: "CadastroId",
                principalTable: "Cadastros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Contas_ContaId",
                table: "Entradas",
                column: "ContaId",
                principalTable: "Contas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_TipoEntradas_TipoEntradaId",
                table: "Entradas",
                column: "TipoEntradaId",
                principalTable: "TipoEntradas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Usuarios_UsuarioId",
                table: "Entradas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Agencias_AgenciaId",
                table: "Saidas",
                column: "AgenciaId",
                principalTable: "Agencias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Cadastros_CadastroId",
                table: "Saidas",
                column: "CadastroId",
                principalTable: "Cadastros",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Contas_ContaId",
                table: "Saidas",
                column: "ContaId",
                principalTable: "Contas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_TipoDespesas_TipoDespesaId",
                table: "Saidas",
                column: "TipoDespesaId",
                principalTable: "TipoDespesas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Usuarios_UsuarioId",
                table: "Saidas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Agencias_AgenciaId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Cadastros_CadastroId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Contas_ContaId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_TipoEntradas_TipoEntradaId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Entradas_Usuarios_UsuarioId",
                table: "Entradas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Agencias_AgenciaId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Cadastros_CadastroId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Contas_ContaId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_TipoDespesas_TipoDespesaId",
                table: "Saidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Saidas_Usuarios_UsuarioId",
                table: "Saidas");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Saidas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "TipoDespesaId",
                table: "Saidas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataReferencia",
                table: "Saidas",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "ContaId",
                table: "Saidas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CadastroId",
                table: "Saidas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "AgenciaId",
                table: "Saidas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataCadastro",
                table: "Recebimentos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataActualizacao",
                table: "Recebimentos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataCadastro",
                table: "ParticipanteEventos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "Entradas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "TipoEntradaId",
                table: "Entradas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ContaId",
                table: "Entradas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CadastroId",
                table: "Entradas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "AgenciaId",
                table: "Entradas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataCadastro",
                table: "EntradaEventos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Agencias_AgenciaId",
                table: "Entradas",
                column: "AgenciaId",
                principalTable: "Agencias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Cadastros_CadastroId",
                table: "Entradas",
                column: "CadastroId",
                principalTable: "Cadastros",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Contas_ContaId",
                table: "Entradas",
                column: "ContaId",
                principalTable: "Contas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_TipoEntradas_TipoEntradaId",
                table: "Entradas",
                column: "TipoEntradaId",
                principalTable: "TipoEntradas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Entradas_Usuarios_UsuarioId",
                table: "Entradas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Agencias_AgenciaId",
                table: "Saidas",
                column: "AgenciaId",
                principalTable: "Agencias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Cadastros_CadastroId",
                table: "Saidas",
                column: "CadastroId",
                principalTable: "Cadastros",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Contas_ContaId",
                table: "Saidas",
                column: "ContaId",
                principalTable: "Contas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_TipoDespesas_TipoDespesaId",
                table: "Saidas",
                column: "TipoDespesaId",
                principalTable: "TipoDespesas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Saidas_Usuarios_UsuarioId",
                table: "Saidas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
