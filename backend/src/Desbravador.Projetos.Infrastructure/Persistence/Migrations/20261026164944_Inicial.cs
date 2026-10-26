using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Desbravador.Projetos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "funcionarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_externo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sobrenome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_funcionarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projetos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    data_termino = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    risco = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    responsavel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exclusao_bloqueada = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projetos", x => x.id);
                    table.ForeignKey(
                        name: "fk_projetos_funcionarios_responsavel_id",
                        column: x => x.responsavel_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "projeto_funcionarios",
                columns: table => new
                {
                    projeto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vinculado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projeto_funcionarios", x => new { x.projeto_id, x.funcionario_id });
                    table.ForeignKey(
                        name: "fk_projeto_funcionarios_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_projeto_funcionarios_projetos_projeto_id",
                        column: x => x.projeto_id,
                        principalTable: "projetos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_funcionarios_id_externo",
                table: "funcionarios",
                column: "id_externo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_projeto_funcionarios_funcionario_id",
                table: "projeto_funcionarios",
                column: "funcionario_id");

            migrationBuilder.CreateIndex(
                name: "ix_projetos_criado_em",
                table: "projetos",
                column: "criado_em");

            migrationBuilder.CreateIndex(
                name: "ix_projetos_responsavel_id",
                table: "projetos",
                column: "responsavel_id");

            migrationBuilder.CreateIndex(
                name: "ix_projetos_status",
                table: "projetos",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "projeto_funcionarios");

            migrationBuilder.DropTable(
                name: "projetos");

            migrationBuilder.DropTable(
                name: "funcionarios");
        }
    }
}
