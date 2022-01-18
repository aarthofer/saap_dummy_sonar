using Apollo.Core.Dal.Common;
using Apollo.Core.Dal.Dao.MappingTables;
using Apollo.Core.Dal.Interface;
using Apollo.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Apollo.Core.Dal.Dao
{
    public class ReservationDao : ApolloDao<Reservation>, IReservationDao
    {

        public ReservationDao(IConnectionFactory connectionFactory, IQueryBuilderFactory qbFactory) : base(connectionFactory, qbFactory)
        {
        }

        public async Task<Reservation> AddSeatsToReservation(Reservation reservation, IEnumerable<CinemaHallSeat> seats)
        {
            CheckValidReservation(reservation);
            foreach (var seat in seats)
            {
                await AddSeatToReservation(reservation, seat, false);
            }

            return await FindByIdAsync(reservation.Id);
        }

        public async Task<Reservation> AddSeatToReservation(Reservation reservation, CinemaHallSeat seat)
        {
            return await AddSeatToReservation(reservation, seat, true);
        }

        public async Task<Reservation> AddSeatToReservation(Reservation reservation, CinemaHallSeat seat, bool reload)
        {
            CheckValidReservation(reservation);
            CheckValidSeat(seat);
            if (!(await IsSeatAvailable(reservation, seat)))
            {
                throw new ArgumentException("seat is already taken and cannot be added to reservation");
            }

            await CreateAsync<ReservationSeat>(new ReservationSeat { HallSeatId = seat.Id, ReservationId = reservation.Id });

            //reload complete reservationObject again
            if (reload)
            {
                return await FindByIdAsync(reservation.Id);
            }
            else //manually load Seats in one query later
            {
                return reservation;
            }

        }

        private async Task<bool> IsSeatAvailable(Reservation reservation, CinemaHallSeat seat)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(ReservationSeat))
                .JoinTable(Column.Create<ReservationSeat>("reservationId"), Column.Create<Reservation>("id"))
                .SetCondition(Column.Create<ReservationSeat>("hallSeatId"), OperationType.Equals, seat.Id)
                .AddAnd(Column.Create<Reservation>("scheduleId"), OperationType.Equals, reservation.ScheduleId);

            return (await template.QueryAsync<ReservationSeat>(qb)).Count() == 0;
        }

        public async Task<IEnumerable<Reservation>> GetReservations(int scheduleId)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(Reservation))
                .SetCondition(Column.Create<Reservation>("scheduleId"), OperationType.Equals, scheduleId);

            IEnumerable<Reservation> reservations = await template.QueryAsync<Reservation>(qb);
            foreach (var reservatoin in reservations)
            {
                await FillReservation(reservatoin);
            }
            return reservations;
        }

        public override async Task<Reservation> CreateAsync(Reservation reservation)
        {
            CheckValidReservation(reservation);

            using TransactionScope scope = new TransactionScope();
            Reservation newReservation = await base.CreateAsync(reservation);

            if (newReservation == null)
            {
                throw new ArgumentException("New Reservation Could not be inserted");
            }

            foreach (var seat in reservation.Seats)
            {
                await AddSeatToReservation(newReservation, seat, false);
            }

            await FillReservation(newReservation);

            scope.Complete();
            return newReservation;
        }

        public override async Task<int> DeleteByIdAsync(int id)
        {
            using TransactionScope scope = new TransactionScope();
            var qb = GetQueryBuilder()
                .Table(typeof(ReservationSeat))
                .QueryType(QueryTypeEnum.DELETE)
                .SetCondition(Column.Create<ReservationSeat>("reservationId"), OperationType.Equals, id);

            int rowsAffected = await template.ExecuteAsync(qb);
            rowsAffected += await base.DeleteByIdAsync(id);
            scope.Complete();

            return rowsAffected;
        }

        public override async Task<IEnumerable<Reservation>> FindAllAsync()
        {
            IEnumerable<Reservation> reservations = await base.FindAllAsync();

            foreach (Reservation reservation in reservations)
            {
                await FillReservation(reservation);
            }
            return reservations;
        }

        public override async Task<Reservation> FindByIdAsync(int id)
        {
            Reservation reservation = await base.FindByIdAsync(id);
            if (reservation == null)
            {
                throw new ArgumentException($"No reservation with id: {id} found");
            }

            await FillReservation(reservation);
            return reservation;
        }

        private async Task FillReservation(Reservation reservation)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(CinemaHallSeat))
                .JoinTable(Column.Create<CinemaHallSeat>("id"), Column.Create<ReservationSeat>("hallSeatId"))
                .JoinTable(Column.Create<ReservationSeat>("reservationId"), Column.Create<Reservation>("id"))
                .SetCondition(Column.Create<Reservation>("id"), OperationType.Equals, reservation.Id);

            reservation.Seats = await template.QueryAsync<CinemaHallSeat>(qb);
        }

        public async Task<Reservation> RemoveSeatFromReservation(Reservation reservation, CinemaHallSeat seat)
        {
            var qb = GetQueryBuilder()
                .Table(typeof(ReservationSeat))
                .QueryType(QueryTypeEnum.DELETE)
                .SetCondition(Column.Create<ReservationSeat>("reservationId"), OperationType.Equals, reservation.Id)
                .AddAnd(Column.Create<ReservationSeat>("hallSeatId"), OperationType.Equals, seat.Id)
                .Limit(1);

            await template.ExecuteAsync(qb);
            return await FindByIdAsync(reservation.Id);
        }

        public override async Task<Reservation> UpdateAsync(Reservation reservation)
        {
            Reservation updatedReservation = await base.UpdateAsync(reservation);

            foreach (var seat in reservation.Seats)
            {
                await AddSeatToReservation(updatedReservation, seat, false);
            }

            await FillReservation(updatedReservation);
            return updatedReservation;
        }

        private void CheckValidSeat(CinemaHallSeat seat)
        {
            if (seat == null || seat.Id <= 0)
            {
                throw new ArgumentException("Cannot add nonexistent seat to reservation");
            }
        }

        private void CheckValidReservation(Reservation reservation)
        {
            if (reservation == null || reservation.Id < 0)
            {
                throw new ArgumentException("Cannot add seat to non existent reservation");
            }
        }
    }
}