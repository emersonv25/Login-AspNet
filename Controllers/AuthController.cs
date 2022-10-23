using Microsoft.AspNetCore.Mvc;
using ApiAuth.Models;
using Microsoft.AspNetCore.Authorization;
using ApiAuth.Services;
using ApiAuth.Services.Interfaces;
using ApiAuth.Models.Object;

namespace ApiAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login, obtem o token do usuário através de seu username e senha
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ViewUser>> LoginAsync([FromBody]ParamLogin login)
        {
            try
            {
                User? user = await _authService.Login(login.Username, login.Password);
                if (user == null)
                {
                    return BadRequest("Usuário ou senha inválidos !");
                }

                if (user.Enabled == false)
                {
                    return Unauthorized("Usuário Inativo !");
                }

                var token = TokenService.GenerateToken(user);

                return new ViewUser(user.Username, user.FullName, user.Email, token); ;
            }
            catch(Exception ex)
            {
                return BadRequest("Não foi possível realizar o login: " + ex.Message);
            }

        }
        /// <summary>
        /// Registra o usuário
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Post([FromBody]ParamRegister user)
        {
            try
            {
                if (user.Username != null && user.Password != null && user.FullName != null)
                {
                    if (user.Password.Length < 4)
                    {
                        return BadRequest("A Senha precisa conter mais de 4 caracteres");
                    }
                    if (user.Username.Length < 4)
                    {
                        return BadRequest("O nome de usuário precisa conter mais de 4 caracteres");
                    }
                    if (user.FullName == "")
                    {
                        return BadRequest("O Nome não pode ser nulo");
                    }
                    if (_authService.GetUser(user.Username) != null)
                    {
                        return BadRequest("Nome de usuário já cadastrado");
                    }
                    if (_authService.GetUserByEmail(user.Email) != null)
                    {
                        return BadRequest("E-mail já cadastrado");
                    }
                }
                else
                {
                    return BadRequest("Dados para o cadastro inválidos !");
                }

                User? newUser = await _authService.Register(user);
                if (newUser == null)
                {
                    return BadRequest("Não foi possivel cadastrar o usuário");
                }

                return Ok("Usuário cadastrado com sucesso !");
            }
            catch (Exception ex)
            {
                return BadRequest("Não foi possível realizar o cadastro: " + ex.Message);
            }

        }
        /// <summary>
        /// Edita um usuário especifico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("admin/update/{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> UpdateAdmin(string username, User usuarioEditado)
        {
            try
            {
                User? usuario = await _authService.PutUserAdm(username, usuarioEditado);
                if (usuario == null)
                {
                    return BadRequest("Usuário não encontrado");
                }

                return Ok("Usuário editado com sucesso !");
            }
            catch (Exception ex)
            {
                return BadRequest("Não foi possivel realizar a atualização: " + ex.Message);
            }

        }
        /// <summary>
        /// Permite o proprio usuário editar seus dados
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update/{id:int}")]
        [Authorize]
        public async Task<ActionResult> Update(User userEdited)
        {
            try
            {
                var identity = HttpContext.User.Identity;
                var username = identity.Name;

                if(string.IsNullOrEmpty(username))
                {
                    return Unauthorized("Falha ao identificar o usuário, favor relogar");
                }

                User? user = await _authService.PutUser(username, userEdited);
                if (user == null)
                {
                    return BadRequest("Falha ao editar usuário");
                }

                return Ok("Usuário editado com sucesso !");

            }
            catch (Exception ex)
            {
                return BadRequest("Não foi possivel realizar a atualização: " + ex.Message);
            }

        }
        /// <summary>
        /// Deleta um usuário especifico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("admin/delete/{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteUserAdm(string username)
        {

            try
            {
                bool user = await _authService.DeleteUser(username);
                if (user == false)
                {
                    return BadRequest("Falha ao deletar usuário");
                }

                return Ok("Usuário deletado com sucesso !");
            }
            catch (Exception ex)
            {
                return BadRequest("Não foi possível excluir o usuário: " + ex.Message);
            }

        }

        /// <summary>
        /// Verifica se está autenticado e retorna as informações do usuario
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("authenticated")]
        [Authorize]
        public async Task<ActionResult<User?>> Authenticated() {
            var username = User.Identity.Name;

            if(string.IsNullOrEmpty(username))
            {
                return BadRequest("Usuário não encontrado");
            }

            var user = await _authService.GetUser(username);

            return  Ok(user);
         }

    }


}
