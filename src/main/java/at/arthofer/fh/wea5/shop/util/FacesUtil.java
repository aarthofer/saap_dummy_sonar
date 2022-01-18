package at.arthofer.fh.wea5.shop.util;

import java.util.Enumeration;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.el.ELContext;
import javax.el.ExpressionFactory;
import javax.el.ValueExpression;
import javax.faces.application.Application;
import javax.faces.context.FacesContext;
import javax.servlet.http.HttpServletRequest;

/**
 * Util class for common JSF methods.
 */
public class FacesUtil {

	private static final String FAILURE_MODEL = "failureModel";

	/**
	 * Returns a JSF managed bean.
	 * 
	 * @param name
	 * @return
	 */
	public static Object getSessionVariable(String name) {
		FacesContext facesContext = FacesContext.getCurrentInstance();
		Application application = facesContext.getApplication();
		ExpressionFactory expressionFactory = application.getExpressionFactory();
		ELContext elContext = facesContext.getELContext();

		ValueExpression valueExpression = expressionFactory.createValueExpression(elContext, "#{" + name + "}",
				Object.class);
		return valueExpression.getValue(elContext);
	}

	/**
	 * Creates a new {@link Failure} object.
	 * 
	 * @param ex
	 * @param logger
	 * @return
	 */
	public static String createFailure(String msg, Exception ex, Logger logger) {
		Failure f = (Failure) getSessionVariable(FAILURE_MODEL);
		if (f != null) {
			f.setException(ex);
			f.setMessage(msg);
		}
		logger.log(Level.SEVERE, ex.getMessage(), ex);
		return Constants.SHOW_FAILURE;
	}

	/**
	 * Creates a new {@link Failure} object.
	 * 
	 * @param ex
	 * @param logger
	 * @return
	 */
	public static String createFailure(Exception ex, Logger logger) {
		return createFailure(ex.getMessage(), ex, logger);
	}

	/**
	 * Returns the request parameter value defined by "name".
	 * 
	 * @param name
	 * @return
	 */
	public static String getRequestParameterValue(String name) {
		HttpServletRequest request = (HttpServletRequest) FacesContext.getCurrentInstance().getExternalContext()
				.getRequest();

		String paramValue = null;
		Enumeration<?> enumeration = request.getParameterNames();
		while (enumeration.hasMoreElements()) {
			String paramName = (String) enumeration.nextElement();
			if (paramName.indexOf(name) > 0) {
				paramValue = request.getParameter(paramName);
			}
		}

		return paramValue;
	}
}
