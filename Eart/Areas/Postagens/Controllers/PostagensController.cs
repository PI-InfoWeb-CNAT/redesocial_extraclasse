﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Eart.Areas.Postagens.Models;
using Eart.Areas.Membros.Models;
using Eart.Areas.Comportamentos.Models;
using Eart.Persistencia.DAL;

namespace Eart.Areas.Postagens.Controllers
{
    public class PostagensController : Controller
    {
        PostagemDAL postagemDAL = new PostagemDAL();
        MembroDAL membroDAL = new MembroDAL();
        CurtidaDAL curtidaDAL = new CurtidaDAL();
        SeguirDAL seguirDAL = new SeguirDAL();
        ComentarioDAL comentarioDAL = new ComentarioDAL();

        private ActionResult ObterVisaoPostagemPorId(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Postagem postagem = postagemDAL.ObterPostagemPorId((long)id);
            if (postagem == null)
            {
                return HttpNotFound();
            }
            return View(postagem);
        }

        private byte[] SetFoto(HttpPostedFileBase foto)
        {
            var bytesFoto = new byte[foto.ContentLength];
            foto.InputStream.Read(bytesFoto, 0, foto.ContentLength);
            return bytesFoto;
        }

        public FileContentResult GetFoto(long id)
        {
            Postagem postagem = postagemDAL.ObterPostagemPorId(id);
            if (postagem != null)
            {
                return File(postagem.Foto, postagem.FotoType);
            }
            return null;
        }

        private ActionResult GravarMembro(Membro membro)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    membroDAL.GravarMembro(membro);
                }
                return View(membro);
            }
            catch
            {
                return View(membro);
            }
        }

        private ActionResult GravarPostagem(Postagem postagem, HttpPostedFileBase foto = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    postagem.Relevancia = ((postagem.Cont_Curtidas * 3)+ (postagem.Cont_Comentarios * 2)) / 5;
                    if (foto != null)
                    {
                        postagem.FotoType = foto.ContentType;
                        postagem.Foto = SetFoto(foto);
                    }
                    postagemDAL.GravarPostagem(postagem);
                    return RedirectToAction("FeedMembrosSeguidos", "Postagens", new { area = "Postagens" });
                }
                return View(postagem);
            }
            catch
            {
                return View(postagem);
            }
        }

        public ActionResult FeedMembrosSeguidos()
        {
            Membro membroLogin = HttpContext.Session["membroLogin"] as Membro;
            if (membroLogin != null)
            {
                ViewBag.MembroLogado = membroLogin.MembroId;
            }
            IQueryable<Postagem> postagens = postagemDAL.ObterPostagensClassificadasPorData();
            foreach (var p in postagens) { 
                p.Curtida = curtidaDAL.ObterPostagensCurtidasPorMembro((long) p.PostagemId, (long) membroLogin.MembroId);
                p.Membro.Seguindo = seguirDAL.ObterMembroSeguido((long)p.MembroId, (long)membroLogin.MembroId);
                GravarPostagem(p);
            }
            return View(postagens);
        }

        public ActionResult FeedPorRelevancia()
        {
            Membro membroLogin = HttpContext.Session["membroLogin"] as Membro;
            if (membroLogin != null)
            {
                ViewBag.MembroLogado = membroLogin.MembroId;
            }
            IQueryable<Postagem> postagens = postagemDAL.ObterPostagensClassificadasPorData();
            List<Postagem> nova_lista = new List<Postagem>();
            foreach (var p in postagens)
            {
                p.Curtida = curtidaDAL.ObterPostagensCurtidasPorMembro((long)p.PostagemId, (long)membroLogin.MembroId);
                GravarPostagem(p);
                if (p.Relevancia >= 5)
                {
                    nova_lista.Add(p);
                }
            }
            return View(nova_lista);
        }

        public ActionResult Create()
        {
            Postagem postagem = new Postagem();
            Membro membroLogin = HttpContext.Session["membroLogin"] as Membro;
            if (membroLogin != null)
            {
                ViewBag.MembroLogado = membroLogin.MembroId;
                postagem.MembroId = membroLogin.MembroId;
            }
            else
            {
                return RedirectToAction("Login", "Account", new { area = "Membros" });
            }
            return View(postagem);
        }

        [HttpPost] 
        [ValidateAntiForgeryToken]
        public ActionResult Create(Postagem postagem, HttpPostedFileBase foto = null)
        {
            postagem.Data = DateTime.Now;
            Membro membro = membroDAL.ObterMembroPorId((long)postagem.MembroId);
            membro.Cont_Posts++;
            GravarMembro(membro);
            return GravarPostagem(postagem, foto);
        }

        public ActionResult Edit(long? id)
        {
            return ObterVisaoPostagemPorId(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Postagem postagem, HttpPostedFileBase foto = null)
        {
            return GravarPostagem(postagem, foto);
        }

        public ActionResult Details(long? id)
        {
            Membro membroLogin = HttpContext.Session["membroLogin"] as Membro;
            if (membroLogin != null)
            {
                ViewBag.MembroLogado = membroLogin.MembroId;
            }
            return ObterVisaoPostagemPorId(id);
        }

        public ActionResult Delete(long? id)
        {
            Membro membroLogin = HttpContext.Session["membroLogin"] as Membro;
            if (membroLogin != null)
            {
                ViewBag.MembroLogado = membroLogin.MembroId;
            }
            return ObterVisaoPostagemPorId(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(long id, FormCollection collection)
        {
            try
            {
<<<<<<< HEAD
                Membro membroLogin = HttpContext.Session["membroLogin"] as Membro;
                if (membroLogin != null)
                {
                   ViewBag.MembroLogado = membroLogin.MembroId;
                }
=======
>>>>>>> main
                IList<Comentario> comentarios = comentarioDAL.ObterComentariosClassificadosPorPostagem(id);
                foreach (var comentario in comentarios)
                {
                    comentarioDAL.EliminarComentario(comentario);
                }
                IList<Curtida> curtidas = curtidaDAL.ObterCurtidasClassificadasPorPostagem(id);
                foreach (var curtida in curtidas)
                {
                    curtidaDAL.EliminarCurtida(curtida);
                }
                Postagem post = postagemDAL.ObterPostagemPorId(id);
                Membro membro = membroDAL.ObterMembroPorId((long)post.MembroId);
                membro.Cont_Posts--;
                GravarMembro(membro);
                Postagem postagem = postagemDAL.EliminarPostagemPorId(id);
                TempData["Message"] = "Postagem excluída com sucesso";
                return RedirectToAction("FeedMembrosSeguidos", "Postagens", new { area = "Postagens" });
            }
            catch
            {
                return View();
            }
        }
    }
}