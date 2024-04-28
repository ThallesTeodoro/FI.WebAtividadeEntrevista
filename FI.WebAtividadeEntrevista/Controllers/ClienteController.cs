using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            ValidateModel(model);

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }

            BoCliente bo = new BoCliente();

            model.Id = bo.Incluir(new Cliente()
            {
                CEP = model.CEP,
                CPF = RemoveNotNumberChars(model.CPF),
                Cidade = model.Cidade,
                Email = model.Email,
                Estado = model.Estado,
                Logradouro = model.Logradouro,
                Nacionalidade = model.Nacionalidade,
                Nome = model.Nome,
                Sobrenome = model.Sobrenome,
                Telefone = model.Telefone
            });

            BoBeneficiario boBeneficiario = new BoBeneficiario();
            foreach (var item in model.Beneficiarios)
            {
                boBeneficiario.Incluir(new Beneficiario()
                {
                    CPF = RemoveNotNumberChars(item.CPF),
                    Nome = item.Nome,
                    IdCliente = model.Id,
                });
            }

            return Json("Cadastro efetuado com sucesso");
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            ValidateModel(model);

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }

            BoCliente bo = new BoCliente();

            bo.Alterar(new Cliente()
            {
                Id = model.Id,
                CEP = model.CEP,
                CPF = RemoveNotNumberChars(model.CPF),
                Cidade = model.Cidade,
                Email = model.Email,
                Estado = model.Estado,
                Logradouro = model.Logradouro,
                Nacionalidade = model.Nacionalidade,
                Nome = model.Nome,
                Sobrenome = model.Sobrenome,
                Telefone = model.Telefone
            });

            BoBeneficiario boBeneficiario = new BoBeneficiario();
            boBeneficiario.ExcluirPeloIdCliente(model.Id);
            foreach (var item in model.Beneficiarios)
            {
                boBeneficiario.Incluir(new Beneficiario()
                {
                    CPF = RemoveNotNumberChars(item.CPF),
                    Nome = item.Nome,
                    IdCliente = model.Id,
                });
            }

            return Json("Cadastro alterado com sucesso");
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();
            Cliente cliente = bo.Consultar(id);
            List<Beneficiario> beneficiarios = boBeneficiario.Consultar(id);
            Models.ClienteModel model = null;

            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    CPF = FormatCpf(cliente.CPF),
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    Beneficiarios = beneficiarios
                        .Select(b => new BeneficiarioModel()
                        {
                            Id = b.Id,
                            CPF = b.CPF,
                            Nome = b.Nome,
                        })
                        .ToList(),
                };
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        private void ValidateModel(ClienteModel model)
        {
            if (CpfIsInvalid(model.CPF))
            {
                ModelState.AddModelError("CPF", "O CPF é inválido.");
            }

            if (!CpfIsUnique(model.CPF, model.Id))
            {
                ModelState.AddModelError("CPF", "O CPF informado já foi cadastrado.");
            }

            if (model.Beneficiarios != null && model.Beneficiarios.Count > 0)
            {
                var cpfsBeneficiarios = model
                    .Beneficiarios
                    .Select(b => b.CPF)
                    .Distinct()
                    .ToList();

                if (cpfsBeneficiarios.Count != model.Beneficiarios.Count)
                {
                    ModelState.AddModelError("Beneficiarios", "Há CPF(s) dos Beneficiários repetido(s).");
                }

                foreach (var item in model.Beneficiarios)
                {
                    if (CpfIsInvalid(item.CPF))
                    {
                        ModelState.AddModelError("CPFBeneficiario", $"O CPF {item.CPF} do beneficiário é inválido.");
                    }
                }
            }
        }

        private bool CpfIsInvalid(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
            {
                return true;
            }

            cpf = RemoveNotNumberChars(cpf);

            if (cpf.Length != 11)
            {
                return true;
            }

            if (new string(cpf[0], 11) == cpf)
            {
                return true;
            }

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(cpf[i].ToString()) * (10 - i);
            }
            int rest = sum % 11;
            int checkDigit1 = rest < 2 ? 0 : 11 - rest;

            // Check the first check digit
            if (int.Parse(cpf[9].ToString()) != checkDigit1)
            {
                return true;
            }

            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += int.Parse(cpf[i].ToString()) * (11 - i);
            }
            rest = sum % 11;
            int checkDigit2 = rest < 2 ? 0 : 11 - rest;

            // Check the second check digit
            if (int.Parse(cpf[10].ToString()) != checkDigit2)
            {
                return true;
            }

            return false;
        }

        private bool CpfIsUnique(string cpf, long? id)
        {
            BoCliente bo = new BoCliente();

            if (id != null)
            {
                if (bo.VerificarExistenciaAlt(RemoveNotNumberChars(cpf), (long)id))
                {
                    return false;
                }

                return true;
            }
            
            if (bo.VerificarExistencia(RemoveNotNumberChars(cpf)))
            {
                return false;
            }
            
            return true;
        }

        private string RemoveNotNumberChars(string str)
        {
            return new string(str.Where(char.IsDigit).ToArray());
        }
    
        private string FormatCpf(string cpf)
        {
            return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
        }
    }
}