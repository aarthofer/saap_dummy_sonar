using Apollo.BLInterface;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.BL
{
    public class ReservationBl : IReservationBl
    {
        private IReservationDao ReservationDao { get; set; }
        private IUserDao UserDao { get; set; }
        private IScheduleDao ScheduleDao { get; set; }
        private IConfiguration ConfigBl { get; set; }
        private IConstraintBl ConstraintBl { get; set; }
        private ICinemaHallDao CinemaHallDao { get; set; }

        public ReservationBl(IReservationDao reservationDao, IUserDao userDao, IScheduleDao scheduleDao, IConfiguration configBl, IConstraintBl constraintBl, ICinemaHallDao cinemaHallDao)
        {
            this.ReservationDao = reservationDao;
            this.UserDao = userDao;
            this.ScheduleDao = scheduleDao;
            this.ConfigBl = configBl;
            this.ConstraintBl = constraintBl;
            this.CinemaHallDao = cinemaHallDao;
        }

        public async Task<Reservation> AddReservationAsync(Reservation reservation)
        {
            if (reservation.Id > 0)
            {
                throw new ArgumentException("Reservation is already in DB");
            }

            if (reservation.ScheduleId < 0)
            {
                throw new ArgumentException("Reservatoin ScheduleID has to be >= 0");
            }

            if (reservation.UserId < 0)
            {
                throw new ArgumentException("Reservation UserID has to be >= 0");
            }

            if (reservation.Seats.Count() <= 0)
            {
                throw new ArgumentException("At least 1 seat has to be added for reservation");
            }

            var user = await UserDao.FindByIdAsync(reservation.UserId);

            if (user == null)
            {
                throw new ArgumentException("User with given id does not exist");
            }

            await CheckReservationSeatsNotAlreadyTaken(reservation);

            return await ReservationDao.CreateAsync(reservation);
        }

        private async Task CheckReservationSeatsNotAlreadyTaken(Reservation reservation)
        {
            IEnumerable<ReservationSeat> ReservationSeats = await GetSeatsAsync(reservation.ScheduleId);

            var takenSeats = ReservationSeats
                .Where(resSeat => reservation.Seats.Contains(resSeat.Seat) && !(resSeat.state == ReservationSeat.State.free));

            if (takenSeats.Count() > 0)
            {
                throw new ArgumentException("Selected seats are already part of a different reservation.");
            }
        }

        public async Task<IEnumerable<ReservationSeat>> GetSeatsAsync(int scheduleId)
        {
            if (scheduleId < 0)
            {
                throw new ArgumentException("Schedule ID has to be >= 0");
            }

            var schedule = await ScheduleDao.FindByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException("The given ScheduleID doesn't exist in DB");
            }

            var reservations = await ReservationDao.GetReservations(scheduleId);
            var cinemaHallSeats = await CinemaHallDao.GetCinemaHallSeats(schedule.CinemaHallId);
            List<ReservationSeat> resSeats = new List<ReservationSeat>();

            foreach (CinemaHallSeat seat in cinemaHallSeats)
            {
                resSeats.Add(new ReservationSeat { Seat = seat, state = ReservationSeat.State.free });
            }
            
            foreach (var reservation in reservations)
            {
                foreach (var seat in reservation.Seats)
                {
                    resSeats
                        .Where(resSeat => resSeat.Seat.SeatNr == seat.SeatNr)
                        .Single()
                        .state = ReservationSeat.State.reserved;
                }
            }
            String constraint = await ConfigBl.GetValueJsonString("Constraint");
            if (!String.IsNullOrEmpty(constraint))
            {
                JObject constraintConfig = (JObject)ConfigBl.DeserializeValue(constraint);

                if (String.Equals(constraintConfig.GetValue("Active")?.ToString(), "true", StringComparison.OrdinalIgnoreCase))
                {
                    ConstraintBl.SetConstraintStrategy(constraintConfig.GetValue("Strategy").ToString());
                    return await ConstraintBl.CalculateSeatConstraints(resSeats);
                }
            }

            return resSeats;
        }

        public async Task<bool> PayReservationAsync(int reservationId)
        {
            if (reservationId < 0)
            {
                throw new ArgumentException("Reservation ID has to be >= 0");
            }

            Reservation reservation = await ReservationDao.FindByIdAsync(reservationId);
            
            if (reservation == null)
            {
                throw new ArgumentException($"Reservation with id {reservationId} does not exist in DB");
            }

            return await PayReservationAsync(reservation);
        }

        public async Task<bool> PayReservationAsync(Reservation reservation)
        {
            if (reservation == null)
            {
                throw new ArgumentNullException("Reservation has to exist");
            }

            if (reservation.Id < 0)
            {
                throw new ArgumentException("Reservation ID has to be >= 0");
            }

            if (reservation.IsPayed)
            {
                throw new ArgumentException("Reservation is already payed");
            }

            reservation.IsPayed = true;
            Reservation updatedRes = await ReservationDao.UpdateAsync(reservation);

            return updatedRes.IsPayed;
        }

        public async Task<int> RemoveReservationAsync(int reservationId)
        {
            if (reservationId < 0)
            {
                throw new ArgumentException("Reservation ID has to be >= 0");
            }

            return await ReservationDao.DeleteByIdAsync(reservationId);
        }

        public async Task<int> RemoveReservationAsync(Reservation reservation)
        {
            if (reservation == null)
            {
                throw new ArgumentNullException("Reservation cannot be null");
            }

            return await RemoveReservationAsync(reservation.Id);
        }
    }
}
