using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using WebJetMoviesApp.Models;
using Newtonsoft.Json.Linq;

namespace WebJetMoviesApp.Controllers
{
    // TO HANDLE ERROR AT CONTROLLER LEVEL
    [HandleError()]
    public class MoviesController : Controller
    {
        /// <summary>
        /// This is the default action for this controller
        /// </summary>
        /// <returns>View</returns>
        #region MoviesList
        public ActionResult MoviesList()
        {
            ViewBag.MovieListCinemaWorld = GetAllMovieList(Consts.WebJetAPICinemaWorld);
            ViewBag.MovieListFilmWorld = GetAllMovieList(Consts.WebJetAPIFilmWorld);
            ViewBag.CompleteListOfMovies = GetCompleteListOfMovies(ViewBag.MovieListCinemaWorld, ViewBag.MovieListFilmWorld);
           
            return View();
        }
        #endregion

        /// <summary>
        /// This function retrieves all the movie list either from Cinema world or Film world
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns>list of movies</returns>
        #region GetAllMovieList
        private List<Movie> GetAllMovieList(string configKey)
        {
            string strResponseJSON = string.Empty;
            string moviesListUrl = string.Empty;
            string requestData = string.Empty;
            List<Movie> movieList = new List<Movie>();

            try
            {
                moviesListUrl = System.Configuration.ConfigurationManager.AppSettings.Get(configKey);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(moviesListUrl);
                request.Method = "GET";
                request.Headers.Add(System.Configuration.ConfigurationManager.AppSettings.Get("TokenKey"), System.Configuration.ConfigurationManager.AppSettings.Get("TokenValue"));
                request.ContentType = "application/json";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                strResponseJSON = reader.ReadToEnd();

                
                var data = JObject.Parse(strResponseJSON)["Movies"];
                movieList = (List<Movie>)JsonConvert.DeserializeObject<IEnumerable<Movie>>(data.ToString());
                
                
            }
            catch (Exception ex)
            {
                // Log the exception in either DB or using Log4net third party tool
            }

            return movieList;

        }
        #endregion


        /// <summary>
        /// This action is used to view the details of a movie
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="movieSource"></param>
        /// <returns></returns>
        #region ViewMovieDetails
        public ActionResult ViewMovieDetails(string movieId, string movieSource)
        {
            try
            {
                MovieDetails movieDetails = new MovieDetails();
                // Can use enumeration here
                if (movieSource == "FW")
                {
                    movieDetails = GetMovieDetailsByMovieId(Consts.WebJetAPIFilmWorldOfAMovie, movieId);
                }
                else
                {
                    movieDetails = GetMovieDetailsByMovieId(Consts.WebJetAPICinemaWorldOfAMovie, movieId);
                }

                ViewBag.MovieDetails = movieDetails;
            }
            catch (Exception ex)
            {
                // Log the exception in either DB or using Log4net third party tool
            }
            return View();
        }
        #endregion

        /// <summary>
        /// This method is used to retrieve details of a movie
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="movieId"></param>
        /// <returns>MovieDetails</returns>
        #region GetMovieDetailsByMovieId
        private MovieDetails GetMovieDetailsByMovieId(string configKey, string movieId)
        {
            string strResponseJSON = string.Empty;
            string moviesListUrl = string.Empty;
            string requestData = string.Empty;
            MovieDetails movieDetails = new MovieDetails();

            try
            {
                moviesListUrl = System.Configuration.ConfigurationManager.AppSettings.Get(configKey);
                moviesListUrl = moviesListUrl + movieId + "/";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(moviesListUrl);
                request.Method = "GET";
                request.Headers.Add(System.Configuration.ConfigurationManager.AppSettings.Get("TokenKey"), System.Configuration.ConfigurationManager.AppSettings.Get("TokenValue"));
                request.ContentType = "application/json";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(resStream);
                strResponseJSON = reader.ReadToEnd();

                movieDetails = JsonConvert.DeserializeObject<MovieDetails>(strResponseJSON);


            }
            catch (Exception ex)
            {
                // Log the exception in either DB or using Log4net third party tool
            }

            return movieDetails;
        }
        #endregion

        /// <summary>
        /// This function is to get all the movie list from both the sources
        /// </summary>
        /// <param name="cinemaWorldMovies"></param>
        /// <param name="filmWorldMovies"></param>
        /// <returns>SelectListItem</returns>
        #region GetCompleteListOfMovies
        private List<SelectListItem> GetCompleteListOfMovies(List<Movie> cinemaWorldMovies, List<Movie> filmWorldMovies)
        {
            List<Movie> completeListOfMovies = new List<Movie>();
            List<SelectListItem> selLstItems = new List<SelectListItem>();

            try
            {
                completeListOfMovies = cinemaWorldMovies;

                foreach (Movie movie in filmWorldMovies)
                {
                    var movieSearchResult = cinemaWorldMovies.Where(s => (s.Title == movie.Title) && (s.Year == movie.Year));

                    if (movieSearchResult == null)
                    {
                        completeListOfMovies.Add(movie);
                    }
                }

                foreach (Movie movie in completeListOfMovies)
                {
                    selLstItems.Add(new SelectListItem() { Text = movie.Title, Value = movie.ID });
                }
            }
            catch(Exception ex)
            {
                // Log the exception in either DB or using Log4net third party tool
            }

            return selLstItems;
        }
        #endregion


        /// <summary>
        /// This function is used to get the cheapest ticket
        /// </summary>
        /// <param name="movieId"></param>
        /// <returns></returns>
        #region GetCheapestSourceInformation
        public string GetCheapestSourceInformation(string movieId)
        {
            List<Movie> movieList = new List<Movie>();
            string cheapestSourceInfo = string.Empty;
            MovieDetails movieDetails = new MovieDetails();
            MovieDetails movieDetailsToCompare = new MovieDetails();
            List<Movie> movieListToCompare = new List<Movie>();
            bool isCinemaWorld = false;

            try
            {
                movieDetails = GetMovieDetailsByMovieId(Consts.WebJetAPIFilmWorldOfAMovie, movieId);
                if (movieDetails.ID == null)
                {
                    isCinemaWorld = true;
                    movieDetails = GetMovieDetailsByMovieId(Consts.WebJetAPICinemaWorldOfAMovie, movieId);
                }

                if (isCinemaWorld)
                {
                    movieListToCompare = GetAllMovieList(Consts.WebJetAPIFilmWorld);
                }
                else
                {
                    movieListToCompare = GetAllMovieList(Consts.WebJetAPICinemaWorld);
                }

                var movieSearchResult = movieListToCompare.Where(s => (s.Title == movieDetails.Title) && (s.Year == movieDetails.Year)).SingleOrDefault();
                if (movieSearchResult == null)
                {
                    if (isCinemaWorld)
                    {
                        cheapestSourceInfo = Consts.CinemaWorldPrice + movieDetails.Price;
                    }
                    else
                    {
                        cheapestSourceInfo = Consts.FilmWorldPrice + movieDetails.Price;
                    }
                }
                else
                {
                    if (isCinemaWorld)
                    {
                        movieDetailsToCompare = GetMovieDetailsByMovieId(Consts.WebJetAPIFilmWorldOfAMovie, movieSearchResult.ID);
                        if (movieDetails.Price > movieDetailsToCompare.Price)
                        {
                            cheapestSourceInfo = Consts.BoldStart + Consts.FilmWorldPrice + movieDetailsToCompare.Price + Consts.BoldEnd;
                        }
                        else if (movieDetailsToCompare.Price > movieDetails.Price)
                        {
                            cheapestSourceInfo = Consts.BoldStart + Consts.CinemaWorldPrice + movieDetails.Price + Consts.BoldEnd;
                        }
                        else
                        {
                            cheapestSourceInfo = Consts.BoldStart + Consts.BothSources + movieDetails.Price + Consts.BoldEnd;
                        }
                    }
                    else
                    {
                        movieDetailsToCompare = GetMovieDetailsByMovieId(Consts.WebJetAPICinemaWorldOfAMovie, movieSearchResult.ID);
                        if (movieDetails.Price > movieDetailsToCompare.Price)
                        {
                            cheapestSourceInfo = Consts.BoldStart + Consts.CinemaWorldPrice + movieDetailsToCompare.Price + Consts.BoldEnd;
                        }
                        else if (movieDetailsToCompare.Price > movieDetails.Price)
                        {
                            cheapestSourceInfo = Consts.BoldStart + Consts.FilmWorldPrice + movieDetails.Price + Consts.BoldEnd;
                        }
                        else
                        {
                            cheapestSourceInfo = Consts.BoldStart + Consts.BothSources + movieDetails.Price + Consts.BoldEnd;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                // Log the exception in either DB or using Log4net third party tool
            }
 
            return cheapestSourceInfo;
        }
        #endregion
    }
}