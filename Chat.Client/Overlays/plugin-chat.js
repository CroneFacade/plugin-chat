nfive.on('ready', (config) => {
	const $window = $('main');
	const $input = $('#input');
	const $textarea = $('textarea');
	let timer = null;

	const hide = () => {
		clearTimeout(timer);

		nfive.send('blur');

		$window.stop().fadeOut(250);
		$input.hide();
	};

	const show = (input, timeout = false) => {
		clearTimeout(timer);

		if (input) $input.show();
		$window.stop().fadeIn(50);
		$textarea.focus();

		if (timeout) {
			timer = setTimeout(hide, 5000);
		}
	};

	$window.stop().fadeOut(0);

	nfive.show();

	autosize($textarea);

	nfive.on('open', () => {
		show(true);
	});

	const history = [];
	let historyPos = 0;
	let historyLive = true;
	let historyCurrent = '';

	$(document).keydown((e) => {
		switch (e.key) {
			case 'Tab':
				e.preventDefault();
				break;
			case 'Escape':
				hide();
				break;
			case 'PageUp':
				$('#messages').scrollTop($('#messages').scrollTop() - ($('#messages').height() / 2));
				break;
			case 'PageDown':
				$('#messages').scrollTop($('#messages').scrollTop() + ($('#messages').height() / 2));
				break;
			case 'ArrowUp':
				e.preventDefault();

				if (historyLive) {
					historyCurrent = $textarea.val();
					historyLive = false;
				}

				historyPos--;
				if (historyPos < 0) historyPos = 0;

				$textarea.val(history[historyPos]).focus()[0].setSelectionRange(history[historyPos].length, history[historyPos].length);
				break;
			case 'ArrowDown':
				e.preventDefault();

				historyPos++;

				if (historyPos > history.length - 1) {
					historyPos = history.length;

					$textarea.val(historyCurrent).focus()[0].setSelectionRange(historyCurrent.length, historyCurrent.length);

					historyLive = true;
					break;
				}

				$textarea.val(history[historyPos]).focus()[0].setSelectionRange(history[historyPos].length, history[historyPos].length);
				break;
		}
	});

	nfive.on('add-message', (message) => {
		$('#messages').append($('<div>').addClass('alert-container').append($('<article>').addClass(`alert-${message.style}`).html(message.message)));

		$('#messages').scrollTop($('#messages').prop('scrollHeight'));

		show(false, true);

		//nfive.send('export', $("#messages").html());
	});

	$textarea.keydown((e) => {
		if (e.key !== 'Enter') return;

		if ($textarea.val() !== '') {
			if (history.length >= config.history) history.shift();

			if (history.length < 1 || history[history.length - 1] !== $textarea.val()) history.push($textarea.val());

			historyPos = history.length;
			historyCurrent = '';

			nfive.send('message', $textarea.val());

			$textarea.val('');
		}

		show(false, true);
		$input.hide();
		nfive.send('blur');
	});

	$textarea.blur(() => {
		$textarea.focus();
	});
});
