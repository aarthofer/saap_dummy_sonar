package swt6.util;

import java.util.Date;
import java.util.Calendar;

public class DateUtil {
  public static Date getDate(int year, int month, int day) {
    return getTime(year, month, day, 0, 0, 0);
  }

  public static Date getTime(int year, int month, int day, int hour, int minute) {
    return getTime(year, month, day, hour, minute, 0);
  }
  
  public static Date getTime(int year, int month, int day, int hour, int minute, int second) {
    Calendar cal = Calendar.getInstance();
    cal.set(year, month-1, day, hour, minute, second);
    cal.set(Calendar.MILLISECOND, 0);

    return cal.getTime();
  }

  public static Date getTime(int hour, int minute) {
    return getTime(hour, minute, 0);
  }
  
  public static Date getTime(int hour, int minute, int second) {
    Calendar cal = Calendar.getInstance();
    cal.set(Calendar.HOUR_OF_DAY, hour);
    cal.set(Calendar.MINUTE, minute);
    cal.set(Calendar.SECOND, second);
    
    return cal.getTime();
  }
  
  public static java.sql.Date toSqlDate(Date date) {
    return new java.sql.Date(date.getTime());
  }
}
