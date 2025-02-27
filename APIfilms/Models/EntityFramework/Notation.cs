﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace APIfilms.Models.EntityFramework
{
    [PrimaryKey(nameof(UtilisateurId), nameof(FilmId))]
    [Table("t_j_notation_not")]
    public partial class Notation
    {
        [Key]
        [Column("utl_id")]
        public int UtilisateurId { get; set; }

        [Key]
        [Column("flm_id")]
        public int FilmId { get; set; }

        [Column("not_note")]
        [Range(minimum: 1, maximum: 5, ErrorMessage = "La note doit être comprise entre 1 et 5")]
        public int Note { get; set; }

        [ForeignKey(nameof(FilmId))]
        [InverseProperty(nameof(Film.NotesFilm))]
        public virtual Film FilmNote { get; set; } = null!;

        [ForeignKey(nameof(UtilisateurId))]
        [InverseProperty(nameof(Utilisateur.NotesUtilisateur))]
        public virtual Utilisateur UtilisateurNotant { get; set; } = null!;
    }
}
